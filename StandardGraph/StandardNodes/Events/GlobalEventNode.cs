using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable, GraphNode("Global Event", "Standard", "Listen to a global event.")]
public class GlobalEventNode : EventNode
{
    public override IGraphEvent GetEventValue(
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData data
    )
    {
        return GetGlobalEvent();
    }

    public virtual IGraphEvent GetGlobalEvent()
    {
        return new GraphEvent();
    }
}
