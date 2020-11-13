using System;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;
using HestiaMaterialImporter.Extensions;

namespace HestiaMaterialImporter.Core
{
    public class ImporterUtils
    {

        public static Material CreateStandardShaderMaterial(string name, Texture2D albedo, Texture2D smoothness, Texture2D normal, Texture2D ambientOcclusion, Texture2D displacement, Texture2D metalness, Texture2D mask)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogWarning("Standard Render Pipeline is not installed, skipping HDRP material");
                return null;
            }
            Material material = new Material(shader);
            material.SetTexture("_MainTex", albedo);
            material.SetTexture("_MetallicGlossMap", mask);
            material.SetTexture("_BumpMap", normal);
            material.SetTexture("_OcclusionMap", ambientOcclusion);
            material.SetTexture("_ParallaxMap", displacement);
            string path = $"Assets/Materials/{name}_Standard.mat";
            AssetDatabase.CreateAsset(material, path);
            return AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
        }

        public static Material CreateHDRPMaterial(string name, Texture2D albedo, Texture2D smoothness, Texture2D normal, Texture2D ambientOcclusion, Texture2D displacement, Texture2D metalness, Texture2D mask)
        {
            Shader shader = Shader.Find("HDRP/Lit");
            if (shader == null)
            {
                Debug.LogWarning("High Definition RP is not installed, skipping HDRP material");
                return null;
            }

            Material material = new Material(shader);
            material.SetTexture("_BaseColorMap", albedo);
            material.SetTexture("_MaskMap", mask);
            material.SetTexture("_NormalMap", normal);
            if (displacement != null)
            {
                material.SetTexture("_HeightMap", displacement);
                material.SetFloat("_DisplacementMode", 2.0f);
            }
            string path = $"Assets/Materials/{name}_HDRP.mat";
            AssetDatabase.CreateAsset(material, path);
            return AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
        }

        public static Material CreateURPMaterial(string name, Texture2D albedo, Texture2D smoothness, Texture2D normal, Texture2D ambientOcclusion, Texture2D displacement, Texture2D metalness, Texture2D mask)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogWarning("Universal RP is not installed, skipping URP material");
                return null;
            }

            Material material = new Material(shader);
            material.SetTexture("_BaseMap", albedo);
            material.SetTexture("_MetallicGlossMap", mask);
            material.SetTexture("_BumpMap", normal);
            material.SetTexture("_OcclusionMap", ambientOcclusion);
            string path = $"Assets/Materials/{name}_URP.mat";
            AssetDatabase.CreateAsset(material, path);
            return AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
        }

        public static Texture2D ImportMaskMap(string name, Texture2D smoothness, Texture2D ambientOcclusion, Texture2D metalness)
        {
            string path = $"Textures/{name}/{name}_Maskmap.png";
            return MaskMap.Create(
                metalness,
                ambientOcclusion,
                null,
                smoothness,
                0.0f,
                1.0f,
                0.0f,
                0.0f).ToAsset(path);
        }
    }
}
