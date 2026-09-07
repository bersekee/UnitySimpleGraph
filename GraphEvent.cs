using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGraphEvent
{
    void Subscribe(Func<INodeData, IGraphContext, GraphBlackboard, Awaitable> listener);

    void Unsubscribe(Func<INodeData, IGraphContext, GraphBlackboard, Awaitable> listener);

    Awaitable Raise(INodeData payload, IGraphContext ctx, GraphBlackboard board);
}

public class GraphEvent : IGraphEvent
{
    private event Func<INodeData, IGraphContext, GraphBlackboard, Awaitable> _listeners;

    public void Subscribe(Func<INodeData, IGraphContext, GraphBlackboard, Awaitable> listener)
    {
        _listeners += listener;
    }

    public void Unsubscribe(Func<INodeData, IGraphContext, GraphBlackboard, Awaitable> listener)
    {
        _listeners -= listener;
    }

    public async Awaitable Raise(INodeData payload, IGraphContext ctx, GraphBlackboard board)
    {
        if (_listeners == null)
            return;

        foreach (var listener in _listeners.GetInvocationList())
        {
            var handler = (Func<INodeData, IGraphContext, GraphBlackboard, Awaitable>)listener;

            await handler(payload, ctx, board);
        }
    }
}
