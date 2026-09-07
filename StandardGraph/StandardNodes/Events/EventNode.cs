using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable, GraphNode("Event", "Standard", "Listen to an event.")]
public class EventNode : EntryNode
{
    [SerializeReference]
    protected List<GraphPart> _parts = new();

    protected Func<INodeData, IGraphContext, GraphBlackboard, Awaitable> _listener;

    protected enum EventPortID
    {
        Event = 2,
    }

    public EventNode()
        : base()
    {
        NodeOutputDataPort<IGraphEvent> eventPort = new(
            (int)EventPortID.Event,
            GetEventValue
#if UNITY_EDITOR
            ,
            "Event",
            PortCapacity.Multi
#endif
        );

        AddPort(eventPort);
    }

    public void BindEvent(IGraphEventDispatcher eventDispatcher, IGraphEvent graphEvent)
    {
        if (graphEvent != null)
        {
            _listener = async (payload, ctx, board) =>
            {
                await eventDispatcher.TriggerEvent(_parts, payload, ctx, board);
            };

            graphEvent.Subscribe(_listener);
        }
    }

    public void UnbindEvent(IGraphEvent graphEvent)
    {
        graphEvent.Unsubscribe(_listener);
    }

    public virtual IGraphEvent GetEventValue(
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData data
    )
    {
        return null;
    }

    public override INodeData RegisterPartData(INodeData entryNodeData)
    {
        return entryNodeData;
    }

#if UNITY_EDITOR
    public void ClearParts()
    {
        _parts.Clear();
    }

    public void AddPart(GraphPart part)
    {
        _parts.Add(part);
    }

    public override Color Color => NodeColors.Event;
#endif

    public override bool ShouldExecuteOnRun => false;
}
