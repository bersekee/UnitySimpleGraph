using System;
using UnityEngine;

#if UNITY_EDITOR
using Unity.GraphToolkit.Editor;
#endif

[Serializable]
public enum ComparisonNodeType
{
    GreatherThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Equal,
    NotEqual,
}

public class ComparisonNodeData : INodeData
{
    public bool ComparisonResult;

    public override object Clone()
    {
        ComparisonNodeData comparisonData = new ComparisonNodeData();
        comparisonData.ComparisonResult = ComparisonResult;
        return comparisonData;
    }
}

[Serializable]
public abstract class ComparisonNode<TValue> : StandardNode
{
    public ComparisonNodeType ComparisonType;

    protected enum PortID
    {
        Input = 1,
        True = 2,
        False = 3,
        Value1 = 4,
        Value2 = 5,
    }

    public ComparisonNode()
        : base()
    {
        NodeFlowPort input = new((int)PortID.Input
#if UNITY_EDITOR
            , "In", PortDirection.Input, PortCapacity.Multi
#endif
        );

        NodeFlowPort outputTrue = new(
            (int)PortID.True
#if UNITY_EDITOR
            ,
            "True",
            PortDirection.Output,
            PortCapacity.Multi
#endif
        );

        NodeFlowPort outputFalse = new(
            (int)PortID.False
#if UNITY_EDITOR
            ,
            "False",
            PortDirection.Output,
            PortCapacity.Multi
#endif
        );

        NodeInputDataPort<TValue> value1 = new((int)PortID.Value1
#if UNITY_EDITOR
            , "Value 1"
#endif
        );

        NodeInputDataPort<TValue> value2 = new((int)PortID.Value2
#if UNITY_EDITOR
            , "Value 2"
#endif
        );

        AddPort(input);
        AddPort(outputTrue);
        AddPort(outputFalse);
        AddPort(value1);
        AddPort(value2);
    }

    public override Awaitable Execute(
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData nodeData
    )
    {
        TValue value1 = GetDataFromPort<TValue>((int)PortID.Value1, ctx, board, nodeData);
        TValue value2 = GetDataFromPort<TValue>((int)PortID.Value2, ctx, board, nodeData);

        bool comparisonResult = CompareValues(value1, value2, ctx, board, nodeData);

        ComparisonNodeData comparisonData = nodeData.Get<ComparisonNodeData>(this);
        comparisonData.ComparisonResult = comparisonResult;

        return GraphAwaitable.CompletedAwaitable;
    }

    public override INodeData RegisterPartData(INodeData entryNodeData)
    {
        return new ComparisonNodeData();
    }

    public override int GetPartsOnPort(
        int portID,
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData dataProvider,
        ref bool async
    )
    {
        ComparisonNodeData nodeData = dataProvider.Get<ComparisonNodeData>(this);

        switch (portID)
        {
            case (int)PortID.True:
                return nodeData.ComparisonResult ? 1 : 0;
            case (int)PortID.False:
                return nodeData.ComparisonResult ? 0 : 1;
            default:
                return 0;
        }
    }

    public abstract bool CompareValues(
        TValue value1,
        TValue value2,
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData nodeData
    );

#if UNITY_EDITOR
    public override Color Color => NodeColors.Flow;
#endif
}

[Serializable, GraphNode("Compare (Int)", "Standard", "Compare multiple integers.")]
public class ComparisonIntNode : ComparisonNode<int>
{
    public override bool CompareValues(
        int value1,
        int value2,
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData nodeData
    )
    {
        switch (ComparisonType)
        {
            case ComparisonNodeType.GreatherThan:
                return value1 > value2;
            case ComparisonNodeType.GreaterThanOrEqual:
                return value1 >= value2;
            case ComparisonNodeType.LessThan:
                return value1 < value2;
            case ComparisonNodeType.LessThanOrEqual:
                return value1 <= value2;
            case ComparisonNodeType.Equal:
                return value1 == value2;
            case ComparisonNodeType.NotEqual:
                return value1 != value2;
            default:
                return false;
        }
    }
}

[Serializable, GraphNode("Compare (Float)", "Standard", "Compare multiple integers.")]
public class ComparisonFloatNode : ComparisonNode<float>
{
    public override bool CompareValues(
        float value1,
        float value2,
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData nodeData
    )
    {
        switch (ComparisonType)
        {
            case ComparisonNodeType.GreatherThan:
                return value1 > value2;
            case ComparisonNodeType.GreaterThanOrEqual:
                return value1 >= value2;
            case ComparisonNodeType.LessThan:
                return value1 < value2;
            case ComparisonNodeType.LessThanOrEqual:
                return value1 <= value2;
            case ComparisonNodeType.Equal:
                return value1 == value2;
            case ComparisonNodeType.NotEqual:
                return value1 != value2;
            default:
                return false;
        }
    }
}
