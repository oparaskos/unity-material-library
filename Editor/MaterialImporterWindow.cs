using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Threading;
using HestiaMaterialImporter.CC0;
using HestiaMaterialImporter.Core;
using HestiaMaterialImporter.Extensions;
using System;
using UnityEngine.UIElements;

namespace HestiaMaterialImporter.Editor
{
    [EditorWindowTitle(title = "Material Importer", icon = "Material Icon")]
    public class MaterialImporterWindow : EditorWindow
    {

        string searchString;
        Vector2 scrollPos;

        [NonSerialized]
        IMaterialsAdapter[] adapters = {
            new CC0MaterialsAdapter()
        };
        ResultLoader loader = null;

        [MenuItem("Window/Material Importer")]
        static void Open()
        {
            MaterialImporterWindow window = GetWindow<MaterialImporterWindow>();
            window.Initialize();
        }

        public void Initialize()
        {
            titleContent = new GUIContent("Material Importer");
            titleContent.image = EditorGUIUtility.IconContent("PreMatSphere", "Material Importer").image;
        }

        private void OnKeyPress(KeyDownEvent evt)
        {
            Debug.Log(evt);
        }

        void OnGUI()
        {

            GUIStyle s = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 25,
            };
            EditorGUILayout.BeginHorizontal("Box");
            string newSearchString = EditorGUILayout.DelayedTextField(searchString, s, GUILayout.Height(30));
            if (
                newSearchString != searchString || 
                GUILayout.Button(EditorGUIUtility.IconContent("Search Icon", "|Search"), GUILayout.Height(30), GUILayout.Width(30)))
            {
                searchString = newSearchString;
                if (loader == null || loader.thread?.ThreadState == ThreadState.Stopped)
                {
                    loader = new ResultLoader()
                    {
                        searchString = searchString,
                        adapters = adapters,
                    };

                    loader.thread = new Thread(new ThreadStart(loader.LoadResults));
                    loader.thread.Start();
                }
            }
            EditorGUILayout.EndHorizontal();

            Rect wnd = EditorGUILayout.BeginVertical();
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scrollView.scrollPosition;
                int numPerRow = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / 120.0f) - 1;
                if (loader == null)
                {
                    EditorGUILayout.LabelField("Type something above to start a search.");
                }
                else if (loader.failed)
                {
                    EditorGUILayout.LabelField("Something went wrong!");
                }
                else if (loader.completed)
                {
                    if (loader.Results.Count() == 0)
                        EditorGUILayout.LabelField($"No Results matched your query {searchString}.");

                    foreach (IEnumerable<IMaterialOption> row in loader.Results.Chunk(numPerRow))
                    {
                        EditorGUILayout.BeginHorizontal();
                        foreach (IMaterialOption option in row)
                        {
                            option.OnGUI();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Loading...");
                }
                EditorGUILayout.LabelField("Please donsider donating to cc0textures.com on patreon");
            }
            EditorGUILayout.EndVertical();
        }
    }
}
