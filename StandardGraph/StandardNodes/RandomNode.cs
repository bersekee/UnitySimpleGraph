using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[
    Serializable,
    GraphNode("Random Number (Int)", "Standard", "Get a random integer between two values.")
]
public class RandomIntNode : FixedDataNode<int>
{
    public int MinValue;

    public int MaxValue;

    public override int GetValue(IGraphContext ctx, GraphBlackboard board, GraphPartData data)
    {
        return UnityEngine.Random.Range(MinValue, MaxValue);
    }
}

[
    Serializable,
    GraphNode("Random Number (Float)", "Standard", "Get a random float between two values.")
]
public class RandomFloatNode : FixedDataNode<float>
{
    public float MinValue;

    public float MaxValue;

    public override float GetValue(IGraphContext ctx, GraphBlackboard board, GraphPartData data)
    {
        return UnityEngine.Random.Range(MinValue, MaxValue);
    }
}
