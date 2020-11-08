using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEditor;

public partial class MaterialImporter : EditorWindow
{
    string searchString;
    Vector2 scrollPos;
    MaterialsAdapter[] adapters = { new CC0MaterialsAdapter() };
    List<MaterialOption> results = new List<MaterialOption>();
    private readonly string _windowTitle = "Material Importer";

    [MenuItem("Window/Material Importer")]
    static void Open()
    {
        MaterialImporter window = GetWindow<MaterialImporter>();
        window.Initialize();
    }

    public void Initialize()
    {
        titleContent = new GUIContent(_windowTitle);

    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal("Box");
        searchString = EditorGUILayout.TextField(searchString, GUILayout.Height(30));
        if (GUILayout.Button(EditorGUIUtility.IconContent("Search Icon", "|Search"),  GUILayout.Height(30), GUILayout.Width(30)))
        {
            results = new List<MaterialOption>();
            foreach(MaterialsAdapter adapter in adapters) {
                results.AddRange(adapter.GetMaterials(searchString));
            }
        }
        EditorGUILayout.EndHorizontal();

        Rect wnd = EditorGUILayout.BeginVertical();
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scrollView.scrollPosition;
                int numPerRow = (int) (wnd.width / 128);
                if (results != null) {
                    foreach(IEnumerable<MaterialOption> row in results.Chunk(numPerRow)) {
                        EditorGUILayout.BeginHorizontal();
                            foreach(MaterialOption option in row) {
                                option.OnGUI();
                            }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        EditorGUILayout.EndVertical();
    }

    public interface MaterialOption {
        string name { get; }
        string[] variants { get; }
        Rect OnGUI();
    }
    public interface MaterialsAdapter {
        List<MaterialOption> GetMaterials(string name);
    }
}
