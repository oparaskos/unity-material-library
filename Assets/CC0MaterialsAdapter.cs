using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEditor;

public class CC0MaterialsAdapter: MaterialImporter.MaterialsAdapter {
    static private Texture2D _favicon = null;

    public Texture2D Favicon {
        get {
            if (_favicon == null) {
                _favicon = LoadTextureFromUri(@"https://icons.duckduckgo.com/ip2/cc0textures.com.ico");
            }
            return _favicon;
        }
    }

    public static Texture2D LoadTextureFromUri(string uri) {
        WebRequest webRequest = WebRequest.Create(uri);
        WebResponse webResp = webRequest.GetResponse();
        var rawData = ReadFully(webResp.GetResponseStream());
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

    public List<MaterialImporter.MaterialOption> GetMaterials(string name) {
        WebRequest webRequest = WebRequest.Create("https://cc0textures.com/api/v1/full_json?q=" + name);
        WebResponse webResp = webRequest.GetResponse();
        StreamReader reader = new StreamReader(webResp.GetResponseStream());
        string responseString = reader.ReadToEnd();
        if(responseString.Contains("\"Assets\": []")) return new List<MaterialImporter.MaterialOption>();
        try {
            CC0Response response = JsonConvert.DeserializeObject<CC0Response>(responseString);
            List<MaterialImporter.MaterialOption> results =  response.Assets
                .Take(10)
                .Select(kvp => new CC0MaterialOption(kvp.Key, kvp.Value, Favicon) as MaterialImporter.MaterialOption)
                .ToList();
            Debug.LogFormat("Got {0} results", results.Count);
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
        private int previewImageSize = 128;
        private int orgImgSize = 16;
    
        public CC0MaterialOption(string name, CC0Asset asset, Texture2D orgImage) {
            this.orgImage = orgImage;
            this.asset = asset;
            this.name = name;
            this.variants = asset.Downloads.Keys.ToArray();
            string preview = asset.PreviewSphere[$"{previewImageSize}-PNG"];
            if(preview == null)
                preview = asset.PreviewSphere[$"{previewImageSize}-JPG"];
            if(preview == null)
                preview = asset.PreviewSphere.Last().Value;
            if(preview != null)
                previewImage = LoadTextureFromUri(preview);
        }

        public Rect OnGUI() {
            var rect = EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.PrefixLabel(new GUIContent(name));
            if(previewImage != null) {
                GUILayout.Box(previewImage, GUILayout.Width(previewImageSize), GUILayout.Height(previewImageSize));
                GUI.DrawTexture(new Rect(rect.x, rect.y, (rect.width*2) - orgImgSize, orgImgSize), orgImage, ScaleMode.ScaleToFit);
            } else {
                GUILayout.Box(orgImage, GUILayout.Width(previewImageSize), GUILayout.Height(previewImageSize));
            }
            EditorGUILayout.EndVertical();
            return rect;
        }
    }
}
