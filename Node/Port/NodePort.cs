using System;
using UnityEngine;
#if UNITY_EDITOR
using Unity.GraphToolkit.Editor;
#endif

#if UNITY_EDITOR
// Graph toolkit definition is unity restricted.
public enum PortCapacity
{
    None,
    Single,
    Multi,
}
#endif

public interface INodePort
{
    public int Id { get; }
    public Type DataType { get; }

#if UNITY_EDITOR
    public string Name { get; }
    public PortDirection Direction { get; }
    public PortCapacity Capacity { get; }
    public Color Color { get; }
#endif
}
