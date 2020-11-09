using System.Linq;
using System.Net;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.IO.Compression;

namespace HestiaMaterialImporter.CC0
{
    public class CC0MaterialOption : BaseMaterialOption
    {
        private CC0Asset asset;

        protected override void DoImport()
        {
            // Ensure folder heirarchy
            EditorUtility.DisplayProgressBar("Importing", "Creating Folders...", 0f);
            string texturePath = $"{Application.dataPath}/Textures/{name}/";
            Debug.Log($"Textures will be stored at '{texturePath}'");
            texturePath.MakeParents();
            string materialPath = $"{Application.dataPath}/Materials/";
            Debug.Log($"Materials will be stored at '{materialPath}'");
            materialPath.MakeParents();

            // Download the ZIP
            string downloadUrl = asset.Downloads[variants[selectedVariant]].RawDownloadLink;
            EditorUtility.DisplayProgressBar("Importing", $"Downloading and importing content from {downloadUrl}", 0f);
            WebRequest webRequest = WebRequest.Create(downloadUrl);
            WebResponse webResp = webRequest.GetResponse();

            // Open the zip archive
            EditorUtility.DisplayProgressBar("Importing", "Downloading and Reading Package Content...", 0.1f);
            ZipArchive package = new ZipArchive(webResp.GetResponseStream());
            Debug.Log(string.Join(", ", package.Entries.Select(it => it.FullName)));
            package.ImportTexturesAndMaterials(name);
        }

        public static async Task<IMaterialOption> Create(string key, CC0Asset value, Task<PreviewImage> favicon)
        {
            int previewImageSize = 128;
            int orgImgSize = 16;
            return new CC0MaterialOption()
            {
                name = key,
                asset = value,
                variants = value.Downloads.Keys.ToArray(),
                selectedVariant = value.Downloads.Keys.Count - 1,
                orgImage = await favicon,
                previewImage = await GrabPreviewImage(value, previewImageSize),
                previewImageSize = previewImageSize,
                orgImgSize = orgImgSize
            };
        }

        private static async Task<PreviewImage> GrabPreviewImage(CC0Asset value, int previewImageSize)
        {
            string preview = value.PreviewSphere[$"{previewImageSize}-PNG"];
            if (preview == null)
                preview = value.PreviewSphere[$"{previewImageSize}-JPG"];
            if (preview == null)
                preview = value.PreviewSphere.Last().Value;
            if (preview != null)
                return await PreviewImage.LoadUri(preview);
            return null;
        }
    }
}