using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using Unity.GraphToolkit.Editor;
#endif

[Serializable, GraphNode("Wait", "Standard", "Wait for a specified amount of seconds.")]
public class WaitNode : StandardNode
{
    protected enum PortID
    {
        Input = 1,
        Output = 2,
        Time = 3,
    }

    public WaitNode()
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

        NodeInputDataPort<float> time = new((int)PortID.Time
#if UNITY_EDITOR
            , "Time"
#endif
        );

        AddPort(input);
        AddPort(output);
        AddPort(time);
    }

    public override async Awaitable Execute(
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData data
    )
    {
        float seconds = GetDataFromPort<float>((int)PortID.Time, ctx, board, data);
        await Awaitable.WaitForSecondsAsync(seconds);
    }

#if UNITY_EDITOR
    public override Color Color => NodeColors.Flow;
#endif
}
