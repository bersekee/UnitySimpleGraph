using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[Serializable]
public abstract class FixedDataNode<TValue> : StandardNode
{
    protected enum PortID
    {
        Value = 1,
    }

    public FixedDataNode()
        : base()
    {
        NodeOutputDataPort<TValue> output = new(
            (int)PortID.Value,
            GetValue
#if UNITY_EDITOR
            ,
            "Value",
            PortCapacity.Multi
#endif
        );

        AddPort(output);
    }

    public abstract TValue GetValue(IGraphContext ctx, GraphBlackboard board, GraphPartData data);

#if UNITY_EDITOR
    public override Color Color => NodeColors.Data;
#endif
}

[Serializable]
public abstract class FixedDataNodeStatic<TValue> : FixedDataNode<TValue>
{
    public TValue Value;

    public override TValue GetValue(IGraphContext ctx, GraphBlackboard board, GraphPartData data)
    {
        return Value;
    }
}

[Serializable, GraphNode("Fixed Data (Int)", "Standard", "Fixed integer data.")]
public class FixedIntDataNode : FixedDataNodeStatic<int> { }

[Serializable, GraphNode("Fixed Data (Int Array)", "Standard", "Fixed integer array data.")]
public class FixedIntArrayDataNode : FixedDataNodeStatic<int[]> { }

[Serializable, GraphNode("Fixed Data (Float)", "Standard", "Fixed float data.")]
public class FixedFloatDataNode : FixedDataNodeStatic<float> { }

[Serializable, GraphNode("Fixed Data (String)", "Standard", "Fixed string data.")]
public class FixedStringDataNode : FixedDataNodeStatic<string> { }
