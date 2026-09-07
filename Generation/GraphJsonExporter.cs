#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GraphJsonExporter
{
    [MenuItem("Assets/Stack/Graph/Export To JSON", true)]
    private static bool ValidateExportGraph()
    {
        UnityEngine.Object selected = Selection.activeObject;

        return selected != null && GraphJsonAssetAdapter.IsGraphAsset(selected);
    }

    [MenuItem("Assets/Stack/Graph/Export To JSON")]
    private static void ExportSelectedGraph()
    {
        UnityEngine.Object selected = Selection.activeObject;

        GraphJson graph = GraphJsonAssetAdapter.BuildJson((BaseGraph)selected);

        if (graph == null)
        {
            Debug.LogError($"Failed to export graph '{selected.name}'.");

            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(selected);

        string directory = Path.GetDirectoryName(sourcePath);

        string fileName = Path.GetFileNameWithoutExtension(sourcePath);

        string outputPath = Path.Combine(directory, fileName + ".json");

        string json = JsonUtility.ToJson(graph, true);

        File.WriteAllText(outputPath, json);

        AssetDatabase.Refresh();

        Debug.Log($"Graph exported to:\n{outputPath}");
    }

    public static string Export(string graphType, List<NodeJson> nodes, List<LinkJson> connections)
    {
        GraphJson graph = new GraphJson
        {
            Version = GraphJsonAssetAdapter.GetCurrentVersion(),
            GraphType = graphType,
            Nodes = nodes ?? new List<NodeJson>(),
            Links = connections ?? new List<LinkJson>(),
        };

        return JsonUtility.ToJson(graph, true);
    }

    public static void ExportToFile(string path, string graphType, List<NodeJson> nodes, List<LinkJson> connections)
    {
        string json = Export(graphType, nodes, connections);

        File.WriteAllText(path, json);

        Debug.Log($"Graph exported to: {path}");
    }
}

#endif