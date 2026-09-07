#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphWindow : EditorWindow
{
    private BaseGraphView _view;
    private GraphBlackboardView _blackboardView;

    private const string LAST_ASSET_KEY = "ABILITY_GRAPH_LAST";

    public static void Open(GraphWindow wnd, BaseGraph graph)
    {
        wnd.titleContent = new GUIContent(graph.name);

        EditorPrefs.SetString(LAST_ASSET_KEY, AssetDatabase.GetAssetPath(graph));

        wnd._view.Populate(graph);

        wnd._blackboardView.Populate(graph.BlacboardData);
    }

    private void OnEnable()
    {
        rootVisualElement.Clear();

        CreateView();

        rootVisualElement.Add(_view);

        GenerateToolbar();

        GenerateBlackBoard();

        string path = EditorPrefs.GetString(LAST_ASSET_KEY, "");

        if (!string.IsNullOrEmpty(path))
        {
            BaseGraph ability = AssetDatabase.LoadAssetAtPath<BaseGraph>(path);
            _view.Populate(ability);
        }
    }

    protected void CreateView()
    {
        _view = new BaseGraphView(this);
    }

    private void GenerateBlackBoard()
    {
        _blackboardView = new GraphBlackboardView();

        rootVisualElement.Add(_blackboardView);
    }

    private void GenerateToolbar()
    {
        Toolbar toolbar = new Toolbar();

        // Spacer pushes everything after it to the right
        VisualElement spacer = new VisualElement();
        spacer.style.flexGrow = 1;

        Button saveButton = new Button(() =>
        {
            SaveGraph();
        });

        saveButton.text = "Save";

        toolbar.Add(spacer);
        toolbar.Add(saveButton);

        rootVisualElement.Add(toolbar);
    }

    private void SaveGraph()
    {
        _view.Save();
    }
}
#endif
