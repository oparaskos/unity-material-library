using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Net;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using HestiaMaterialImporter.Extensions;
using HestiaMaterialImporter.Core;

namespace HestiaMaterialImporter.CC0
{
    public class CC0MaterialsAdapter : IMaterialsAdapter
    {
        static private Task<PreviewImage> favicon = PreviewImage.LoadFavicon(@"cc0textures.com");
        public PreviewImage Favicon { get { return favicon.Result; } }
        public string statusLine = null;

        public void OnActivate() {}

        public void OnGUI() {
            EditorGUILayout.LabelField("Please donsider donating to cc0textures.com on patreon");
            if (statusLine != null)
                EditorGUILayout.LabelField(statusLine);
        }

        public async Task<IEnumerable<Task<IMaterialOption>>> GetMaterials(string name)
        {
            string responseString = "None";
            try
            {
                statusLine = "Searching cc0textures.com";
                WebRequest webRequest = WebRequest.Create($"https://cc0textures.com/api/v1/full_json?q={name}&type=PhotoTexturePBR&limit=10&sort=Popular");
                WebResponse webResp = await webRequest.GetResponseAsync();
                statusLine = "Got response from cc0textures.com";
                
                StreamReader reader = new StreamReader(webResp.GetResponseStream());
                responseString = await reader.ReadToEndAsync();
                statusLine = "Reading response from cc0textures.com";

                if (responseString.Contains("\"Assets\": []")) return new List<Task<IMaterialOption>>();
                statusLine = "Processing response from cc0textures.com";
                
                CC0Response response = JsonConvert.DeserializeObject<CC0Response>(responseString);
                IEnumerable<Task<IMaterialOption>> results
                    = response.Assets
                        .Take(10)
                        .Select(kvp => CC0MaterialOption.Create(kvp.Key, kvp.Value, favicon));
                statusLine = String.Format("Got {0} results from cc0textures.com", response.Assets.Count);
                return results;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(responseString);
                throw e;
            }
        }
    }
}