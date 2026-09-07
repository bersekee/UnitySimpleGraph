using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
#if UNITY_EDITOR
using Unity.GraphToolkit.Editor;
#endif

public class LoopNodeData<TInput, TOutput> : INodeData
{
    public TInput List;

    public override object Clone()
    {
        LoopNodeData<TInput, TOutput> loopData = new LoopNodeData<TInput, TOutput>();
        loopData.List = List;
        return loopData;
    }
}

[Serializable]
public abstract class LoopNode<TInput, TOutput> : StandardNode
    where TInput : IEnumerable<TOutput>
{
    public bool Async;

    protected enum PortID
    {
        Input = 1,
        Output = 2,
        OutValue = 3,
        InList = 4,
    }

    public LoopNode()
        : base()
    {
        NodeFlowPort input = new((int)PortID.Input
#if UNITY_EDITOR
            , "In", PortDirection.Input, PortCapacity.Multi
#endif
        );

        NodeFlowPort output = new(
            (int)PortID.Output
#if UNITY_EDITOR
            ,
            "Out",
            PortDirection.Output,
            PortCapacity.Multi
#endif
        );

        NodeInputDataPort<TInput> inValue = new((int)PortID.InList
#if UNITY_EDITOR
            , "List"
#endif
        );

        NodeOutputDataPort<TOutput> outValue = new(
            (int)PortID.OutValue,
            GetValue
#if UNITY_EDITOR
            ,
            "Value",
            PortCapacity.Multi
#endif
        );

        AddPort(input);
        AddPort(output);
        AddPort(inValue);
        AddPort(outValue);
    }

    public override Awaitable Execute(
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData partData
    )
    {
        TInput value = GetDataFromPort<TInput>((int)PortID.InList, ctx, board, partData);
        LoopNodeData<TInput, TOutput> loopData = partData.Get<LoopNodeData<TInput, TOutput>>(this);
        loopData.List = value;

        return GraphAwaitable.CompletedAwaitable;
    }

    public override INodeData RegisterPartData(INodeData entryNodeData)
    {
        return new LoopNodeData<TInput, TOutput>();
    }

    public override int GetPartsOnPort(
        int portID,
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData data,
        ref bool async
    )
    {
        LoopNodeData<TInput, TOutput> loopData = data.Get<LoopNodeData<TInput, TOutput>>(this);
        if (loopData.List == null)
        {
            return 0;
        }
        return loopData.List.Count();
    }

    public TOutput GetValue(IGraphContext ctx, GraphBlackboard board, GraphPartData data)
    {
        LoopNodeData<TInput, TOutput> loopData = data.Get<LoopNodeData<TInput, TOutput>>(this);
        return loopData.List.ElementAt(data.PartIndex);
    }

#if UNITY_EDITOR
    public override bool ForceBreakIntoParts => true;
    public override Color Color => NodeColors.Flow;
#endif
}

[Serializable, GraphNode("Loop (Int)", "Standard", "Loop on an integer array.")]
public class LoopIntArrayNode : LoopNode<int[], int> { }
