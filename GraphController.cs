using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//Je taime
[ExecuteAlways]
public class GraphController : MonoBehaviour
{
    [SerializeField]
    private BaseGraph _graph;

    [SerializeField]
    private bool _nodeDebug = true;

    [SerializeField]
    private bool _useStopWatch = true;

    private List<GraphInstance> _runners = new();

    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= OnEditorUpdate;
#endif
    }

#if UNITY_EDITOR
    private void OnEditorUpdate()
    {
        _ = StandardGraphEvents.UpdateEditor.Raise(null, null, null);
    }
#endif

    void Start() { }

    void Update()
    {
        UpdateEventNodeData updateData = new();
        updateData.Delta = Time.deltaTime;
        _ = StandardGraphEvents.Update.Raise(updateData, null, null);
    }

    public Awaitable Run(
        BaseGraph graph,
        StandardGraphContext ctx,
        GraphBlackboard board,
        bool nodeDebug = false,
        bool useStopWatch = false
    )
    {
        GraphInstance instance = new(graph, nodeDebug, useStopWatch);

        return Run(instance, ctx, board);
    }

    public Awaitable Run(GraphInstance runner, StandardGraphContext ctx, GraphBlackboard board)
    {
        _runners.Add(runner);

        return runner.Run(ctx, board);
    }

    [ContextMenu("Execute Graph")]
    private async void ExecuteGraphCommand()
    {
        StandardGraphContext ctx = new();
        GraphBlackboard board = new();

        await Run(_graph, ctx, board, _nodeDebug, _useStopWatch);
    }

    [ContextMenu("Dispose Graphs")]
    private void DisposeGraphsCommand()
    {
        foreach (GraphInstance runner in _runners)
        {
            runner.Dispose();
        }
        _runners.Clear();
    }
}
