using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Net;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.IO.Compression;
using UnityEngine.UIElements;
using UnityEngine.Assertions.Must;
using UnityEngine.Experimental.Rendering;

public class CC0MaterialsAdapter: MaterialImporter.MaterialsAdapter {
    static private byte[] _favicon = null;

    public async Task<byte[]> GetFavicon() {
        if (_favicon == null) {
            _favicon = await BytesFromUri(@"https://icons.duckduckgo.com/ip2/cc0textures.com.ico");
        }
        return _favicon;
    }
    public async static Task<byte[]> BytesFromUri(string uri)
    {
        WebRequest webRequest = WebRequest.Create(uri);
        WebResponse webResp = await webRequest.GetResponseAsync();
        return await ReadFullyAsync(webResp.GetResponseStream());
    }

    public static Texture2D bytesToTexture(byte[] rawData) {
        Texture2D img = new Texture2D(2, 2);
        img.alphaIsTransparency = true;
        img.LoadImage(rawData);
        return img;
    }

    public static byte[] ReadFully(Stream input)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }

    public static async Task<byte[]> ReadFullyAsync(Stream input)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            await input.CopyToAsync(ms);
            return ms.ToArray();
        }
    }

    public async Task<List<MaterialImporter.MaterialOption>> GetMaterials(string name) {
        WebRequest webRequest = WebRequest.Create($"https://cc0textures.com/api/v1/full_json?q={name}&type=PhotoTexturePBR&limit=10&sort=Popular");
        WebResponse webResp = await webRequest.GetResponseAsync();
        StreamReader reader = new StreamReader(webResp.GetResponseStream());
        string responseString = await reader.ReadToEndAsync();
        if(responseString.Contains("\"Assets\": []")) return new List<MaterialImporter.MaterialOption>();
        try {
            CC0Response response = JsonConvert.DeserializeObject<CC0Response>(responseString);
            List<MaterialImporter.MaterialOption> results
                = Task.WhenAll(response.Assets
                    .Take(10)
                    .Select(kvp => CC0MaterialOption.Create(kvp.Key, kvp.Value, GetFavicon()))
                   ).Result.ToList();
            Debug.LogFormat("Got {0} results", response.Assets.Count);
            return results;
        } catch (Exception e) {
            Debug.LogError(responseString);
            throw e;
        }
    }

    public class CC0Response {
        public Dictionary<string, CC0Asset> Assets;
    }

    public class CC0Asset {
        public string Tags;
        public string AssetReleasedate;
        public string DownloadCount;
        public string AssetDataTypeID;
        public string CreationMethodID;
        public string CreatedUsingAssetID = null;
        public string PopularityScore;
        public string Weblink;
        public Dictionary<string, string> PreviewSphere;
        public Dictionary<string, CC0Download> Downloads;
    }

    public class CC0Download {
        public string Filetype;
        public string Size;
        public string PrettyDownloadLink;
        public string RawDownloadLink;
    }

    public class CC0MaterialOption: MaterialImporter.MaterialOption {
        public string name { get; internal set; }
        public string[] variants { get; internal set; }
        private CC0Asset asset;
        private Texture2D orgImage;
        private Texture2D previewImage;
        private byte[] baOrgImage;
        private byte[] baPreviewImage;
        private int previewImageSize = 128;
        private int orgImgSize = 16;
        private int selectedVariant;
        private class ImageNames
        {
            internal string[] names;
            internal string canonicalName;

            internal class Result
            {
                public bool shouldInvert = false;
                public ZipArchiveEntry entry;
            }

            internal Result Find(ZipArchive archive)
            {
                var candidates = archive.Entries
                    .Where(it => names.Any(target => it.Name.ToLower().Contains(target.ToLower())));
                if (candidates.Count() == 0) return null;

                if(candidates.Count() > 1)
                {
                    Debug.LogWarning($"More than one candidate for {names}, {string.Join(", ", candidates.Select(it => it.Name))}");
                }
                if (candidates.Count() == 0) return null;
                var candidate = candidates.First();
                if (this == Smoothness && candidate.Name.ToLower().Contains("rough"))
                    return new Result() { shouldInvert = true, entry = candidate };
                return new Result() { entry = candidate };
            }

            private static readonly string[] albedoNames = { "color", "base", "albedo", "colour" };
            public static readonly ImageNames Albedo = new ImageNames() { canonicalName = "Albedo", names = albedoNames };

            private static readonly string[] roughnessNames = { "roughness", "rough", "smooth", "smoothness" };
            public static readonly ImageNames Smoothness = new ImageNames() { canonicalName = "Smoothness", names = roughnessNames };

            private static readonly string[] normalNames = { "normal", "nrm", "tangent" };
            public static readonly ImageNames Normal = new ImageNames() { canonicalName = "Normal", names = normalNames };

            private static readonly string[] aoNames = { "ao", "ambientocclusion", "ambient_occlusion" };
            public static readonly ImageNames AmbientOcclusion = new ImageNames() { canonicalName = "AmbientOcclusion", names = aoNames };

            private static readonly string[] dispNames = { "disp", "displacement", "bump" };
            public static readonly ImageNames Displacement = new ImageNames() { canonicalName = "Displacement", names = dispNames };

            private static readonly string[] metalNames = { "metal", "metalness" };
            public static readonly ImageNames Metalness = new ImageNames() { canonicalName = "Metalness", names = metalNames };

        };

        public Rect OnGUI() {
            EditorGUI.BeginChangeCheck();
            var rect = EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.PrefixLabel(new GUIContent(name));
            if(previewImage != null) {
                GUILayout.Box(previewImage, GUILayout.Width(previewImageSize), GUILayout.Height(previewImageSize));
                GUI.DrawTexture(new Rect(rect.x, rect.y, (rect.width*2) - orgImgSize, orgImgSize), orgImage, ScaleMode.ScaleToFit);
            } else {
                GUILayout.Box(orgImage, GUILayout.Width(previewImageSize), GUILayout.Height(previewImageSize));
            }
            selectedVariant = EditorGUILayout.Popup(selectedVariant, variants);
            if (GUILayout.Button(new GUIContent("Import")))
            {
                this.Import();
            }
            EditorGUILayout.EndVertical();
            EditorGUI.EndChangeCheck();
            return rect;
        }

        private void Import()
        {
            // Download the ZIP
            EditorUtility.DisplayProgressBar("Importing", "Downloading and importing content...", 0f);
            string downloadUrl = asset.Downloads[variants[selectedVariant]].RawDownloadLink;

            WebRequest webRequest = WebRequest.Create(downloadUrl);
            WebResponse webResp = webRequest.GetResponse();
            EditorUtility.DisplayProgressBar("Importing", "Downloading content...", 0.1f);
            ZipArchive package = new ZipArchive(webResp.GetResponseStream());
            Debug.Log(String.Join(", ", package.Entries.Select(it => it.FullName)));
            EditorUtility.DisplayProgressBar("Importing", "Extracting Albedo...", 0.2f);
            Texture2D albedo = ExtractFromZip(package, ImageNames.Albedo);
            EditorUtility.DisplayProgressBar("Importing", "Smoothness...", 0.3f);
            Texture2D smoothness = ExtractFromZip(package, ImageNames.Smoothness);
            EditorUtility.DisplayProgressBar("Importing", "Normal...", 0.4f);
            Texture2D normal = ExtractFromZip(package, ImageNames.Normal);
            EditorUtility.DisplayProgressBar("Importing", "Ambient Occlusion...", 0.5f);
            Texture2D ambientOcclusion = ExtractFromZip(package, ImageNames.AmbientOcclusion);
            EditorUtility.DisplayProgressBar("Importing", "Displacement...", 0.6f);
            Texture2D displacement = ExtractFromZip(package, ImageNames.Displacement);
            EditorUtility.DisplayProgressBar("Importing", "Metalness...", 0.7f);
            Texture2D metalness = ExtractFromZip(package, ImageNames.Metalness);


            EditorUtility.DisplayProgressBar("Importing", "Generating Maskmap...", 0.8f);
            Texture2D maskMap = Texture2DExt.MaskMap(metalness, ambientOcclusion, smoothness);
            string path = $"Assets/Textures/{name}/{name}_Maskmap.png";
            File.WriteAllBytes(path, maskMap.EncodeToPNG());
            AssetDatabase.ImportAsset(path);

            //EditorUtility.DisplayProgressBar("Importing", "Generating Material...", 0.9f);
            

            EditorUtility.DisplayProgressBar("Importing", "Finished", 1.0f);
        }


        private Texture2D ExtractFromZip(ZipArchive package, ImageNames type)
        {
            string targetDir = Application.dataPath;
            ImageNames.Result result = type.Find(package);
            if (result == null) return null;
            string originalName = result.entry.Name;
            string extension = originalName.Substring(originalName.LastIndexOf("."));

            Texture2D texture = new Texture2D(2, 2);
            if (type == ImageNames.Albedo)
                texture.alphaIsTransparency = true;
            texture.LoadImage(ReadFully(result.entry.Open()));
            if (result.shouldInvert)
                texture = texture.Inverted();
            texture.Apply();

            string path = $"Assets/Textures/{name}/{name}_{type.canonicalName}{extension}";
            string folder = Directory.GetParent($"{Application.dataPath}/{path}").ToString();
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            if (extension.ToLower() == ".jpg")
                File.WriteAllBytes(path, texture.EncodeToJPG());
            else if (extension.ToLower() == ".png")
                File.WriteAllBytes(path, texture.EncodeToJPG());
            else if (extension.ToLower() == ".exr")
                File.WriteAllBytes(path, texture.EncodeToEXR());
            else if (extension.ToLower() == ".tga")
                File.WriteAllBytes(path, texture.EncodeToTGA());
            else throw new BadImageFormatException();
            AssetDatabase.ImportAsset(path);
            return texture;
        }

        public static async Task<MaterialImporter.MaterialOption> Create(string key, CC0Asset value, Task<byte[]> favicon)
        {
            int previewImageSize = 128;
            int orgImgSize = 16;
            byte[] previewImage = null;
            string preview = value.PreviewSphere[$"{previewImageSize}-PNG"];
            if (preview == null)
                preview = value.PreviewSphere[$"{previewImageSize}-JPG"];
            if (preview == null)
                preview = value.PreviewSphere.Last().Value;
            if (preview != null)
                previewImage = await BytesFromUri(preview);
            return new CC0MaterialOption()
            {
                name = key,
                asset = value,
                variants = value.Downloads.Keys.ToArray(),
                selectedVariant = value.Downloads.Keys.Count - 1,
                baOrgImage = await favicon,
                baPreviewImage = previewImage,
                previewImageSize = previewImageSize,
                orgImgSize = orgImgSize

            };
        }

        public MaterialImporter.MaterialOption InitOnMainThread()
        {
            if(orgImage == null)
                orgImage = bytesToTexture(baOrgImage);
            if (previewImage == null)
                previewImage = bytesToTexture(baPreviewImage);
            return this;
        }
    }
}
