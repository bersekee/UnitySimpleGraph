using System;
using UnityEngine;

public class UpdateEventNodeData : INodeData
{
    public float Delta;

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}

[Serializable, GraphNode("Update Event", "Standard", "Event everytime the game is updated.")]
public class UpdateEventNode : GlobalEventNode
{
    protected enum UpdateEventPortID
    {
        delta = 4,
    }

    public UpdateEventNode()
        : base()
    {
        NodeOutputDataPort<float> output = new(
            (int)UpdateEventPortID.delta,
            GetDelta
#if UNITY_EDITOR
            ,
            "Delta",
            PortCapacity.Multi
#endif
        );

        AddPort(output);
    }

    public float GetDelta(IGraphContext ctx, GraphBlackboard board, GraphPartData data)
    {
        UpdateEventNodeData updateData = data.Get<UpdateEventNodeData>(this);
        return updateData.Delta;
    }

    public override IGraphEvent GetGlobalEvent()
    {
        return StandardGraphEvents.Update;
    }
}
