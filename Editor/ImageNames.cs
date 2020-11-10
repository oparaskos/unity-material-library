using System.Linq;
using UnityEngine;
using System.IO.Compression;
using UnityEditor;

namespace HestiaMaterialImporter
{
    public class ImageNames
    {
        internal string[] names;
        internal string canonicalName;
        public TextureImporterType importType = TextureImporterType.Default;

        internal class Result
        {
            public bool shouldInvert = false;
            public ZipArchiveEntry entry;
        }

        internal Result FindInPackage(string name, ZipArchive archive)
        {
            var candidates = archive.Entries
                .Where(it => it.Name.ToLower().Contains(name.ToLower()))
                .Where(it => names.Any(target => it.Name.ToLower().Contains(target.ToLower())));

            if (candidates.Count() == 0) return null;

            if (candidates.Count() > 1)
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
        public static readonly ImageNames Normal = new ImageNames() { canonicalName = "Normal", names = normalNames, importType = TextureImporterType.NormalMap };

        private static readonly string[] aoNames = { "ao", "ambientocclusion", "ambient_occlusion" };
        public static readonly ImageNames AmbientOcclusion = new ImageNames() { canonicalName = "AmbientOcclusion", names = aoNames };

        private static readonly string[] dispNames = { "disp", "displacement", "bump" };
        public static readonly ImageNames Displacement = new ImageNames() { canonicalName = "Displacement", names = dispNames };

        private static readonly string[] metalNames = { "metal", "metalness" };
        public static readonly ImageNames Metalness = new ImageNames() { canonicalName = "Metalness", names = metalNames };

    };
}