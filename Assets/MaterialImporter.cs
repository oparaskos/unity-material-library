using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

public partial class MaterialImporter : EditorWindow
{
    string searchString;
    Vector2 scrollPos;
    MaterialsAdapter[] adapters = { new CC0MaterialsAdapter() };
    private readonly string _windowTitle = "Material Importer";
    ResultLoader loader = null;

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
    class ResultLoader
    {
        public Thread thread = null;
        private IEnumerable<MaterialOption> _results;
        public bool completed = false;
        public bool failed = false;
        public string searchString;
        public MaterialsAdapter[] adapters;

        public IEnumerable<MaterialOption> Results {
            get {
                return _results?.Select(it => it.InitOnMainThread());
            }
        }

        public void LoadResults()
        {
            try
            {
                _results = Task.WhenAll(adapters
                    .Select(it => it.GetMaterials(searchString))).Result.SelectMany(it => it);
                completed = true;
            } catch (Exception e)
            {
                failed = true;
            }
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal("Box");
        searchString = EditorGUILayout.TextField(searchString, GUILayout.Height(30));
        if (GUILayout.Button(EditorGUIUtility.IconContent("Search Icon", "|Search"), GUILayout.Height(30), GUILayout.Width(30)))
        {
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
                if(loader.Results.Count() == 0)
                    EditorGUILayout.LabelField($"No Results matched your query {searchString}.");

                foreach (IEnumerable<MaterialOption> row in loader.Results.Chunk(numPerRow))
                {
                    EditorGUILayout.BeginHorizontal();
                    foreach (MaterialOption option in row)
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
        }
        EditorGUILayout.EndVertical();
    }

    public interface MaterialOption
    {
        string name { get; }
        string[] variants { get; }
        Rect OnGUI();
        MaterialOption InitOnMainThread();
    }
    public interface MaterialsAdapter
    {
        Task<List<MaterialOption>> GetMaterials(string name);
    }
}
