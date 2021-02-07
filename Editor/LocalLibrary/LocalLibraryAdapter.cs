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
using System.Text.RegularExpressions;

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

        public async Task<List<Task<IMaterialOption>>> GetMaterialsInPath(string path, string search)
        {
            var results = new List<Task<IMaterialOption>>();
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists) {
                Debug.LogWarningFormat("Library Path '{0}' does not exist", path);
                return results;
            }

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Attributes.HasFlag(FileAttributes.Directory)) continue;
                if (file.Extension != ".zip") continue;

                string name = file.Name.Trim();
                name = Regex.Replace(name, @".zip$", "", RegexOptions.IgnoreCase).Trim();
                name = Regex.Replace(name, @"[_\-.]", " ", RegexOptions.IgnoreCase).Trim();
                name = Regex.Replace(name, @"\b(jpg|png)\b", "", RegexOptions.IgnoreCase).Trim();
                name = Regex.Replace(name, @"\b\dk\b", "", RegexOptions.IgnoreCase).Trim();
                name = Regex.Replace(name, @"([A-Z0-9]+)", " $1").Trim();
                name = Regex.Replace(name, @"\s+", " ").Trim();

                string searchString = Regex.Replace(search ?? " ", @"\s+", " ")?.Trim()?.ToLower();
                if (!name.ToLower().Contains(searchString)) continue;
    
                results.Add(LocalMaterialOption.Create(name, favicon, file));
            }
            return results;

        }
        
        public async Task<IEnumerable<Task<IMaterialOption>>> GetMaterials(string name)
        {
            if (settings?.m_LocalLibraryPaths != null) {
                IEnumerable<Task<List<Task<IMaterialOption>>>> tasks = settings.m_LocalLibraryPaths
                    .ToList()
                    .Select(path => GetMaterialsInPath(path, name));
                List<Task<IMaterialOption>> results = (await Task.WhenAll(tasks))
                    .SelectMany(i => i)
                    .ToList();
                return results;
            }
            return new List<Task<IMaterialOption>>();
        }
    }
}