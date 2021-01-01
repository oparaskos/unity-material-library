using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using HestiaMaterialImporter.Extensions;

namespace HestiaMaterialImporter.Editor
{
    class HestiaSettings : ScriptableObject
    {
        [SerializeField]
        public List<string> m_LocalLibraryPaths;

        public static HestiaSettings GetOrCreateSettings()
        {
            HestiaSettingsRegister.HestiaSettingsPath.MakeParents();
            var settings = AssetDatabase.LoadAssetAtPath<HestiaSettings>(HestiaSettingsRegister.HestiaSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<HestiaSettings>();
                settings.m_LocalLibraryPaths = new List<string>();
                AssetDatabase.CreateAsset(settings, HestiaSettingsRegister.HestiaSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}