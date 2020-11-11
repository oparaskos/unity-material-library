using System.IO.Compression;
using UnityEditor;
using UnityEngine;
using HestiaMaterialImporter.Core;

namespace HestiaMaterialImporter.Extensions
{
	public static class ZipArchiveExt
    {
        public static Texture2D ImportFromZip(this ZipArchive package, string name, ImageNames type)
        {
            ImageNames.Result result = type.FindInPackage(name, package);

            if (result == null) return null;

            string originalName = result.entry.Name;
            string extension = originalName.Substring(originalName.LastIndexOf("."));

            Texture2D texture = new Texture2D(2, 2);
            if (type == ImageNames.Albedo)
                texture.alphaIsTransparency = true;
            texture.LoadImage(result.entry.Open().ReadFully());
            if (result.shouldInvert)
                texture = texture.Inverted();
            texture.Apply();

            return texture.ToAsset($"Textures/{name}/{name}_{type.canonicalName}{extension}", type.importType);
        }
        public static void ImportTexturesAndMaterials(this ZipArchive package, string name)
        {
            // Extract each image from the zip
            EditorUtility.DisplayProgressBar("Importing", "Extracting Albedo...", 0.2f);
            Texture2D albedo = package.ImportFromZip(name, ImageNames.Albedo);
            EditorUtility.DisplayProgressBar("Importing", "Smoothness...", 0.3f);
            Texture2D smoothness = package.ImportFromZip(name, ImageNames.Smoothness);
            EditorUtility.DisplayProgressBar("Importing", "Normal...", 0.4f);
            Texture2D normal = package.ImportFromZip(name, ImageNames.Normal);
            EditorUtility.DisplayProgressBar("Importing", "Ambient Occlusion...", 0.5f);
            Texture2D ambientOcclusion = package.ImportFromZip(name, ImageNames.AmbientOcclusion);
            EditorUtility.DisplayProgressBar("Importing", "Displacement...", 0.6f);
            Texture2D displacement = package.ImportFromZip(name, ImageNames.Displacement);
            EditorUtility.DisplayProgressBar("Importing", "Metalness...", 0.7f);
            Texture2D metalness = package.ImportFromZip(name, ImageNames.Metalness);

            // Generate Maskmap for HDRP
            EditorUtility.DisplayProgressBar("Importing", "Generating Maskmap...", 0.8f);
            Texture2D mask = ImporterUtils.ImportMaskMap(name, smoothness, ambientOcclusion, metalness);

            EditorUtility.DisplayProgressBar("Importing", "Generating Material...", 0.9f);
            ImporterUtils.CreateStandardShaderMaterial(name, albedo, smoothness, normal, ambientOcclusion, displacement, metalness, mask);
            EditorUtility.DisplayProgressBar("Importing", "Generating Material for HDRP", 0.95f);
            ImporterUtils.CreateHDRPMaterial(name, albedo, smoothness, normal, ambientOcclusion, displacement, metalness, mask);
            EditorUtility.DisplayProgressBar("Importing", "Generating Material for URP", 0.97f);
            ImporterUtils.CreateURPMaterial(name, albedo, smoothness, normal, ambientOcclusion, displacement, metalness, mask);
            EditorUtility.DisplayProgressBar("Importing", "Finished", 1.0f);
        }
    }
}