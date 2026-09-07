#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

public static class GraphJsonImporter
{
    [MenuItem("Assets/Stack/Graph/Import From JSON", true)]
    private static bool ValidateImportGraph()
    {
        UnityEngine.Object selected = Selection.activeObject;

        if (selected == null)
            return false;

        string path = AssetDatabase.GetAssetPath(selected);

        return Path.GetExtension(path).ToLowerInvariant() == ".json";
    }

    [MenuItem("Assets/Stack/Graph/Import From JSON")]
    private static void ImportSelectedGraph()
    {
        UnityEngine.Object selected = Selection.activeObject;

        if (selected == null)
        {
            Debug.LogError("No JSON file selected.");
            return;
        }

        string jsonPath = AssetDatabase.GetAssetPath(selected);

        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"JSON file does not exist: {jsonPath}");

            return;
        }

        string json = File.ReadAllText(jsonPath);

        GraphJson graph;

        try
        {
            graph = JsonUtility.FromJson<GraphJson>(json);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Failed to parse graph JSON:\n{exception}");

            return;
        }

        if (graph == null)
        {
            Debug.LogError($"Failed to deserialize graph JSON: {jsonPath}");

            return;
        }

        if (string.IsNullOrEmpty(graph.Name))
        {
            Debug.LogError("Graph JSON does not contain a Name.");

            return;
        }

        if (string.IsNullOrEmpty(graph.GraphType))
        {
            Debug.LogError("Graph JSON does not contain a GraphType.");

            return;
        }

        BaseGraph importedGraph;

        try
        {
            importedGraph = GraphJsonAssetAdapter.CreateGraph(graph);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Failed to create graph:\n{exception}");

            return;
        }

        if (importedGraph == null)
        {
            Debug.LogError($"Failed to create graph of type '{graph.GraphType}'.");

            return;
        }

        try
        {
            GraphJsonAssetAdapter.PopulateGraph(importedGraph, graph);
        }
        catch (System.Exception exception)
        {
            UnityEngine.Object.DestroyImmediate(importedGraph);

            Debug.LogError($"Failed to populate graph:\n{exception}");

            return;
        }

        string jsonDirectory = Path.GetDirectoryName(jsonPath);

        string graphPath = Path.Combine(jsonDirectory, graph.Name + ".asset");

        /*
         * Prevent accidentally overwriting an existing graph.
         */
        if (File.Exists(graphPath))
        {
            bool overwrite = EditorUtility.DisplayDialog(
                    "Graph Already Exists",
                    $"A graph named '{graph.Name}' already exists.\n\n" +
                    $"Do you want to replace it?",
                    "Replace",
                    "Cancel");

            if (!overwrite)
            {
                UnityEngine.Object.DestroyImmediate(importedGraph);

                return;
            }

            AssetDatabase.DeleteAsset(graphPath);
        }

        AssetDatabase.CreateAsset(importedGraph, graphPath);

        EditorUtility.SetDirty(importedGraph);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = importedGraph;

        EditorGUIUtility.PingObject(importedGraph);

        Debug.Log($"Graph imported from:\n{jsonPath}\n\n" + $"Graph created at:\n{graphPath}");
    }

    public static GraphJson Import(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new System.ArgumentException("Graph JSON is empty.");

        GraphJson graph = JsonUtility.FromJson<GraphJson>(json);

        if (graph == null)
            throw new System.Exception("Failed to deserialize graph JSON.");

        return graph;
    }

    public static GraphJson ImportFromFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Graph file not found: {path}");

        return Import(File.ReadAllText(path));
    }
}

#endif