using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using HestiaMaterialImporter.Extensions;
using HestiaMaterialImporter.Core;
using System.Text.RegularExpressions;

namespace HestiaMaterialImporter.Local
{
    public class LocalMaterialOption : BaseMaterialOption
    {

        public FileInfo file;
        protected override void DoImport(string texturePath, string materialPath)
        {
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                EditorUtility.DisplayProgressBar("Importing", "Reading Package Content...", 0.1f);
                ZipArchive package = new ZipArchive(fileStream, ZipArchiveMode.Read);
                Debug.Log(string.Join(", ", package.Entries.Select(it => it.FullName)));
                package.ImportTexturesAndMaterials(name);
            }
        }
        public static async Task<IMaterialOption> Create(string key, PreviewImage favicon, FileInfo file)
        {
            int previewImageSize = 128;
            int orgImgSize = 16;
            return new LocalMaterialOption()
            {
                file = file,
                name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key),
                variants = FindVariants(file).ToArray(),
                selectedVariant = 0,
                orgImage = favicon,
                previewImage = await GrabPreviewImage(previewImageSize),
                previewImageSize = previewImageSize,
                orgImgSize = orgImgSize
            };
        }

        private static async Task<PreviewImage> GrabPreviewImage(int previewImageSize)
        {
            // TODO: Look in the archive to see if there is something like a preview.jpg

            return PreviewImage.LoadTextureAtPath("Packages/com.github.oparaskos.unity.hestia.material.importer/Editor/CompressedFolder.png");
        }

        private static IEnumerable<string> FindVariants(FileInfo file)
        {
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                ZipArchive package = new ZipArchive(fileStream, ZipArchiveMode.Read);
                return package.Entries
                    .Select(it => FileNameToVariantName(it.FullName))
                    .Where(it => it != null);
            }
        }

        private static string FileNameToVariantName(string fileName) {
            if (!(fileName.EndsWith("jpg") || fileName.EndsWith("exr") || fileName.EndsWith("png") || fileName.EndsWith("tga") || fileName.EndsWith("jpeg"))) {
                return null;
            }

            if (fileName.Contains("/")) {
                return fileName.Substring(0, fileName.LastIndexOf("/"));
            }
            
            string name = fileName.Trim();
            name = Regex.Replace(name, @"\.?(jpg|png|tga|exr|jpeg)([_\- \b]|$)", "", RegexOptions.IgnoreCase).Trim();
            name = Regex.Replace(name, @"(^|[\b_\- ])\dk(?=[_\- \b]|$)", "", RegexOptions.IgnoreCase).Trim();
            name = Regex.Replace(name, @"(^|[\b_\- ])((scattering|opacity|metalness|disp|displacement|normal|roughness|diffuse|diff|nor|col|color|colour|rough|ao|ambient|occlusion|ambientocclusion|smooth|smoothness)(?=[_\- \b]|$))+", "", RegexOptions.IgnoreCase).Trim();name = Regex.Replace(name, @"(^|[\b_\- ])((disp|displacement|normal|roughness|diffuse|diff|nor|col|color|colour|rough|ao|ambient|occlusion|ambientocclusion|smooth|smoothness)([_\- \b]|$))+", "", RegexOptions.IgnoreCase).Trim();name = Regex.Replace(name, @"(^|[\b_\- ])((disp|displacement|normal|roughness|diffuse|diff|nor|col|color|colour|rough|ao|ambient|occlusion|ambientocclusion|smooth|smoothness)([_\- \b]|$))+", "", RegexOptions.IgnoreCase).Trim();
            name = Regex.Replace(name, @"_+", "_").Trim();
            name = Regex.Replace(name, @"\s+", " ").Trim();
            name = Regex.Replace(name, @"-+", "-").Trim();
            name = Regex.Replace(name, @"[-_]+$", "").Trim();
            return name;
        }

    }
}