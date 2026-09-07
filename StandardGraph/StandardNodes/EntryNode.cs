using System;
using UnityEngine;
#if UNITY_EDITOR
using Unity.GraphToolkit.Editor;
#endif

[Serializable, GraphNode("Entry", "Standard", "Base entry for the graph.")]
public class EntryNode : StandardNode
{
    protected enum PortID
    {
        Output = 1,
    }

    public EntryNode()
        : base()
    {
        NodeFlowPort output = new NodeFlowPort(
            (int)PortID.Output
#if UNITY_EDITOR
            ,
            "Out",
            PortDirection.Output,
            PortCapacity.Multi
#endif
        );

        AddPort(output);
    }

    public override Awaitable Execute(IGraphContext ctx, GraphBlackboard board, GraphPartData data)
    {
        return GraphAwaitable.CompletedAwaitable;
    }

    public virtual bool ShouldExecuteOnRun => true;

#if UNITY_EDITOR
    public override Color Color => NodeColors.Entry;
#endif
}
