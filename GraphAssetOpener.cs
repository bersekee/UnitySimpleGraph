#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class GraphAssetOpener
{
    [OnOpenAsset]
    public static bool OnOpen(EntityId entityId, int line)
    {
        BaseGraph graph = EditorUtility.EntityIdToObject(entityId) as BaseGraph;

        if (graph != null)
        {
            GraphWindow wnd = EditorWindow.GetWindow(typeof(GraphWindow)) as GraphWindow;
            GraphWindow.Open(wnd, graph);
            return true;
        }

        return false;
    }
}
#endif
