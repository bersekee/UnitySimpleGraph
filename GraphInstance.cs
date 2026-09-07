using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class GraphInstance : IGraphEventDispatcher, IDisposable
{
    BaseGraph _graph;

#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
    bool _nodeDebug;
    bool _timeStamp;
    System.Diagnostics.Stopwatch _stopwatch;
#endif

    public GraphInstance(BaseGraph graph
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
        , bool nodeDebug = false, bool timestamp = false
#endif
    )
    {
        _graph = graph;

#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
        _nodeDebug = nodeDebug;
        _timeStamp = timestamp;
#endif

        if (_graph == null)
        {
            UnityEngine.Debug.LogError("Graph is null!!");
            return;
        }

        // Register global events.
        foreach (GraphPart part in _graph.GraphParts)
        {
            GlobalEventNode eventEntry = part.EntryNode as GlobalEventNode;
            if (eventEntry != null)
            {
                eventEntry.BindEvent(this, eventEntry.GetGlobalEvent());
            }
        }
    }

    public Awaitable Run(IGraphContext ctx, GraphBlackboard board)
    {
        if (_graph == null)
            return GraphAwaitable.CompletedAwaitable;

        return Run(_graph.EntryParts, null, ctx, board);
    }

    public void BindEvent<T>(IGraphEvent graphEvent)
        where T : EventNode
    {
        foreach (GraphPart part in _graph.GraphParts)
        {
            if (part.EntryNode is T eventEntry)
            {
                eventEntry.BindEvent(this, graphEvent);
            }
        }
    }

    public Awaitable TriggerEvent(
        List<GraphPart> parts,
        INodeData entryNodeData,
        IGraphContext ctx,
        GraphBlackboard board
    )
    {
        return Run(parts, entryNodeData, ctx, board);
    }

    private async Awaitable Run(
        List<GraphPart> entryParts,
        INodeData entryNodeData,
        IGraphContext ctx,
        GraphBlackboard board
    )
    {
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
        StartStopWatch();
#endif

        using (
            var pooledObject = ListPool<GraphPartInstance>.Get(
                out List<GraphPartInstance> partsToRun
            )
        )
        {
            // Temp add all entry parts
            foreach (GraphPart part in entryParts)
            {
                GraphPartData nodeData = GraphNodeDataPool.Get(_graph, 0, true);
                nodeData.CreateData(_graph, entryNodeData);

                partsToRun.Add(new GraphPartInstance { Part = part, PartData = nodeData });
            }

            List<Awaitable> awaitables = new List<Awaitable>();
            for (int i = 0; i < partsToRun.Count; i++)
            {
                GraphPartInstance currentState = partsToRun[i];
                if (currentState.PartData.Async)
                {
                    Awaitable awaitable = ExecuteGraphPart(currentState, ctx, board, partsToRun);
                    awaitables.Add(awaitable);
                }
                else
                {
                    await WaitForParts(awaitables);

                    await ExecuteGraphPart(currentState, ctx, board, partsToRun);
                }
            }

            await WaitForParts(awaitables);

            // Dispose and recycle all generated objects back into the pool
            for (int i = 0; i < partsToRun.Count; i++)
            {
                GraphPartData data = partsToRun[i].PartData;
                GraphNodeDataPool.Release(data);
            }
        }

#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
        StopStopWatch();
#endif
    }

    private async Awaitable ExecuteGraphPart(
        GraphPartInstance currentState,
        IGraphContext context,
        GraphBlackboard board,
        List<GraphPartInstance> partsToRun
    )
    {
        GraphPart graphPart = currentState.Part;
        List<BaseNode> partNodes = graphPart.Nodes;

        GraphPartData nodeData = currentState.PartData;

        for (int e = 0; e < partNodes.Count; e++)
        {
            BaseNode node = partNodes[e];

#if UNITY_EDITOR
            BeginNodeDebug(node);
#endif

            await node.Execute(context, board, nodeData);

            GraphPartLink[] partLinks = node.PartLinks;
            foreach (GraphPartLink partLink in partLinks)
            {
                bool async = nodeData.Async;
                int count = node.GetPartsOnPort(partLink.Port, context, board, nodeData, ref async);

                for (int j = 0; j < count; j++)
                {
                    GraphPartData newNodeData = GraphNodeDataPool.Get(_graph, j, async);
                    newNodeData.Copy(nodeData);

                    partsToRun.Add(
                        new GraphPartInstance { Part = partLink.Part, PartData = newNodeData }
                    );
                }
            }

#if UNITY_EDITOR
            EndNodeDebug(node);
#endif
        }
    }

    private async Awaitable WaitForParts(List<Awaitable> awaitables)
    {
        foreach (Awaitable awaitable in awaitables)
        {
            await awaitable;
        }
        awaitables.Clear();
    }

    public void Dispose()
    {
        foreach (GraphPart part in _graph.GraphParts)
        {
            GlobalEventNode eventEntry = part.EntryNode as GlobalEventNode;
            if (eventEntry != null)
            {
                eventEntry.UnbindEvent(eventEntry.GetGlobalEvent());
            }
        }
    }

#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
    private void StartStopWatch()
    {
        if (!_timeStamp)
            return;

        UnityEngine.Debug.Log($"Nodegraph Execution Start.");
        _stopwatch = new System.Diagnostics.Stopwatch();
        _stopwatch.Start();
    }

    private void StopStopWatch()
    {
        if (!_timeStamp)
            return;

        _stopwatch.Stop();
        UnityEngine.Debug.Log(
            $"Nodegraph Execution Time: {(double)_stopwatch.ElapsedTicks * 1_000_000 / System.Diagnostics.Stopwatch.Frequency} us"
        );
    }
#endif

#if UNITY_EDITOR
    private void BeginNodeDebug(BaseNode node)
    {
        if (!_nodeDebug)
            return;

        NodeExecutionEvents.OnNodeStarted?.Invoke(node.UniqueId);
    }

    private void EndNodeDebug(BaseNode node)
    {
        if (!_nodeDebug)
            return;

        NodeExecutionEvents.OnNodeFinished?.Invoke(node.UniqueId);
    }
#endif

    public BaseGraph Graph => _graph;
}
