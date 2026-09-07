using System;
using UnityEngine;

[Serializable, GraphNode("Update Event", "Standard", "Event everytime the editor is updated.")]
public class EditorUpdateEventNode : GlobalEventNode
{
    public EditorUpdateEventNode()
        : base() { }

    public override IGraphEvent GetGlobalEvent()
    {
        return StandardGraphEvents.UpdateEditor;
    }
}
