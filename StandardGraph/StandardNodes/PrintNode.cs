using System;
using UnityEngine;
#if UNITY_EDITOR
using Unity.GraphToolkit.Editor;
#endif

[Serializable]
public abstract class PrintNode<TValue> : StandardNode
{
    protected enum PortID
    {
        Input = 1,
        Output = 2,
        Print = 3,
    }

    public PrintNode()
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
            "Output",
            PortDirection.Output,
            PortCapacity.Multi
#endif
        );

        NodeInputDataPort<TValue> print = new((int)PortID.Print
#if UNITY_EDITOR
            , "Print"
#endif
        );

        AddPort(input);
        AddPort(output);
        AddPort(print);
    }

    public override Awaitable Execute(IGraphContext ctx, GraphBlackboard board, GraphPartData data)
    {
        Debug.Log(GetDataFromPort<TValue>((int)PortID.Print, ctx, board, data));

        return GraphAwaitable.CompletedAwaitable;
    }

#if UNITY_EDITOR
    public override Color Color => NodeColors.Standard;
#endif
}

[Serializable, GraphNode("Print (Int)", "Standard", "Print an integer.")]
public class PrintIntNode : PrintNode<int> { }

[Serializable, GraphNode("Print (String)", "Standard", "Print a string.")]
public class PrintStringNode : PrintNode<string> { }

[Serializable, GraphNode("Print (Float)", "Standard", "Print a float.")]
public class PrintFloatNode : PrintNode<float> { }
