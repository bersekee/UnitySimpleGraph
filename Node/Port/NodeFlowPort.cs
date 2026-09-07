using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

public class NodeFlowPort : INodePort
{
    private int _id;

#if UNITY_EDITOR
    private string _name;
    private PortDirection _direction;
    private PortCapacity _capacity;
#endif

    public NodeFlowPort(int id
#if UNITY_EDITOR
        , string name, PortDirection direction, PortCapacity capacity
#endif
    )
    {
        _id = id;

#if UNITY_EDITOR
        _capacity = capacity;
        _direction = direction;
        _name = name;
#endif
    }

    public int Id => _id;
    public Type DataType => null;

#if UNITY_EDITOR
    public PortCapacity Capacity => _capacity;
    public PortDirection Direction => _direction;
    public string Name => _name;
    public Color Color => Color.white;
#endif
}
