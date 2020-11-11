using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Net;
using UnityEngine;
using System.Threading.Tasks;
using HestiaMaterialImporter.Extensions;
using HestiaMaterialImporter.Core;

namespace HestiaMaterialImporter.CC0
{
    public class CC0MaterialsAdapter : IMaterialsAdapter
    {
        static private Task<PreviewImage> favicon = PreviewImage.LoadFavicon(@"cc0textures.com");
        public PreviewImage Favicon { get { return favicon.Result; } }

        public async Task<List<IMaterialOption>> GetMaterials(string name)
        {
            WebRequest webRequest = WebRequest.Create($"https://cc0textures.com/api/v1/full_json?q={name}&type=PhotoTexturePBR&limit=10&sort=Popular");
            WebResponse webResp = await webRequest.GetResponseAsync();
            StreamReader reader = new StreamReader(webResp.GetResponseStream());
            string responseString = await reader.ReadToEndAsync();
            if (responseString.Contains("\"Assets\": []")) return new List<IMaterialOption>();
            try
            {
                CC0Response response = JsonConvert.DeserializeObject<CC0Response>(responseString);
                List<IMaterialOption> results
                    = Task.WhenAll(response.Assets
                        .Take(10)
                        .Select(kvp => CC0MaterialOption.Create(kvp.Key, kvp.Value, favicon))
                       ).Result.ToList();
                // Debug.LogFormat("Got {0} results", response.Assets.Count);
                return results;
            }
            catch (Exception e)
            {
                Debug.LogError(responseString);
                throw e;
            }
        }
    }
}