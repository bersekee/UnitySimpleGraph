using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

public abstract class NodeDataPort<TValue> : INodePort
{
    private int _id;

#if UNITY_EDITOR
    private string _name;
    private PortCapacity _capacity;
#endif

    public NodeDataPort(int id
#if UNITY_EDITOR
        , string name, PortCapacity capacity
#endif
    )
    {
        _id = id;

#if UNITY_EDITOR
        _capacity = capacity;
        _name = name + " (" + DataType.Name + ")";
#endif
    }

    public int Id => _id;
    public Type DataType => typeof(TValue);

#if UNITY_EDITOR
    public abstract PortDirection Direction { get; }
    public PortCapacity Capacity => _capacity;
    public string Name => _name;
    public Color Color
    {
        get
        {
            int hash = DataType.FullName?.GetHashCode() ?? DataType.Name.GetHashCode();
            byte r = (byte)((hash & 0xFF0000) >> 16);
            byte g = (byte)((hash & 0x00FF00) >> 8);
            byte b = (byte)(hash & 0x0000FF);
            return new Color(r / 255f, g / 255f, b / 255f);
        }
    }
#endif
}

public class NodeOutputDataPort<TValue> : NodeDataPort<TValue>
{
    private Func<IGraphContext, GraphBlackboard, GraphPartData, TValue> _getter;

    public NodeOutputDataPort(
        int id,
        Func<IGraphContext, GraphBlackboard, GraphPartData, TValue> getter
#if UNITY_EDITOR
        ,
        string name,
        PortCapacity capacity
#endif
    )
        : base(id
#if UNITY_EDITOR
            , name, capacity
#endif
        )
    {
        _getter = getter;
    }

    public TValue GetValue(IGraphContext ctx, GraphBlackboard board, GraphPartData data)
    {
        return _getter(ctx, board, data);
    }

    public override PortDirection Direction => PortDirection.Output;
}

public class NodeInputDataPort<TValue> : NodeDataPort<TValue>
{
    public NodeInputDataPort(int id
#if UNITY_EDITOR
        , string name
#endif
    )
        : base(id
#if UNITY_EDITOR
            , name, PortCapacity.Single
#endif
        ) { }

    public override PortDirection Direction => PortDirection.Input;
}
