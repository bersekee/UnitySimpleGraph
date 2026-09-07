using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[Serializable]
public abstract class AddNode<TValue> : StandardNode
{
    public TValue AddValue;

    protected enum PortID
    {
        Input = 1,
        Output = 2,
    }

    public AddNode()
        : base()
    {
        NodeInputDataPort<TValue> input = new((int)PortID.Input
#if UNITY_EDITOR
            , "In"
#endif
        );

        NodeOutputDataPort<TValue> output = new(
            (int)PortID.Output,
            CalculateValue
#if UNITY_EDITOR
            ,
            "Out",
            PortCapacity.Multi
#endif
        );

        AddPort(input);
        AddPort(output);
    }

    public abstract TValue CalculateValue(
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData data
    );

#if UNITY_EDITOR
    public override Color Color => NodeColors.Data;
#endif
}

[Serializable, GraphNode("Add (Int)", "Standard", "Add two integer numbers.")]
public class AddIntNode : AddNode<int>
{
    public override int CalculateValue(IGraphContext ctx, GraphBlackboard board, GraphPartData data)
    {
        return GetDataFromPort<int>((int)PortID.Input, ctx, board, data) + AddValue;
    }
}

[Serializable, GraphNode("Add (Float)", "Standard", "Add two floating numbers.")]
public class AddFloatNode : AddNode<float>
{
    public override float CalculateValue(
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData data
    )
    {
        return GetDataFromPort<float>((int)PortID.Input, ctx, board, data) + AddValue;
    }
}
