using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace HestiaMaterialImporter.Editor
{
    class HestiaSettingsProvider : SettingsProvider
    {
        private SerializedObject hestiaSettings;

        class Styles
        {
            public static GUIContent localLibraryPaths = new GUIContent("Local Library Paths");
        }

        public HestiaSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) {}

        public static bool IsSettingsAvailable()
        {
            return File.Exists(HestiaSettingsRegister.HestiaSettingsPath);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the Hestia element in the Settings window.
            hestiaSettings = HestiaSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            // Use IMGUI to display UI:
            EditorGUILayout.PropertyField(hestiaSettings.FindProperty("m_LocalLibraryPaths"), Styles.localLibraryPaths);
            if (GUILayout.Button(new GUIContent("Add Local Library Path"))) {
                string userFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
                string path = EditorUtility.OpenFolderPanel("Add Local Library Path", userFolder, "Materials");
                if (path?.Length > 0) {
                    HestiaSettings.GetOrCreateSettings().m_LocalLibraryPaths.Add(path);
                    hestiaSettings = HestiaSettings.GetSerializedSettings();
                }
            }
            hestiaSettings.ApplyModifiedProperties();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateHestiaSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider = new HestiaSettingsProvider("Project/HestiaSettings", SettingsScope.Project);

                // Automatically extract all keywords from the Styles.
                provider.label = "Hestia | Material Importer";
                provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();

                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }
    }
}