using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGraphEventDispatcher
{
    void BindEvent<T>(IGraphEvent graphEvent)
        where T : EventNode;

    Awaitable TriggerEvent(
        List<GraphPart> parts,
        INodeData entryNodeData,
        IGraphContext ctx,
        GraphBlackboard board
    );
}
