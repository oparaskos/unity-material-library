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
using HestiaMaterialImporter.Editor;

namespace HestiaMaterialImporter.Local
{
    public class LocalLibraryAdapter : IMaterialsAdapter
    {
        static private PreviewImage favicon = PreviewImage.LoadIcon("Folder Icon");
        public PreviewImage Favicon { get { return favicon; } }
        HestiaSettings settings;
        
        public void OnActivate() {
            settings = HestiaSettings.GetOrCreateSettings();
        }

        public void OnGUI() {
            if (settings?.m_LocalLibraryPaths?.Count() > 0)
            {
                string x = String.Format("Including local library paths: {0}", String.Join(", ", settings.m_LocalLibraryPaths));
                EditorGUILayout.LabelField(x);
            }
        }

        public async Task<List<IMaterialOption>> GetMaterialsInPath(string path)
        {
            var example = await LocalMaterialOption.Create("test", favicon);
            return new List<IMaterialOption>() { example };
        }
        
        public async Task<IEnumerable<IMaterialOption>> GetMaterials(string name)
        {
            if (settings?.m_LocalLibraryPaths != null) {
                IEnumerable<Task<List<IMaterialOption>>> tasks = settings.m_LocalLibraryPaths.ToList().Select(path => GetMaterialsInPath(path));
                List<IMaterialOption> results = (await Task.WhenAll(tasks))
                    .SelectMany(i => i)
                    .ToList();
                return results;
            }
            return new List<IMaterialOption>();
        }
    }
}