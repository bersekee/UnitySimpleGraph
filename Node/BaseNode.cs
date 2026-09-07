using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.GraphToolkit.Editor;
using UnityEngine;

#if UNITY_EDITOR
[Serializable]
public class NodeLink
{
    [ReadOnly]
    public int FromNodeId;

    [ReadOnly]
    public int FromPortId;

    [ReadOnly]
    public int ToNodeId;

    [ReadOnly]
    public int ToPortId;
}
#endif

[Serializable]
public class GraphNodeDataLink
{
    [SerializeReference]
    public BaseNode FromNode;

    public int FromPortId;
}

[Serializable]
public abstract class BaseNode
{
    public static readonly int MAX_PORT_COUNT = 10;

    [SerializeField, ReadOnly]
    private int _uniqueId = 0;

    [SerializeField, ReadOnly]
    private int _runtimeId = 0;

    [SerializeField]
    private GraphNodeDataLink[] _dataLinks = new GraphNodeDataLink[MAX_PORT_COUNT];

    [SerializeField]
    private GraphPartLink[] _partLinks = new GraphPartLink[0];

    private INodePort[] _ports = new INodePort[MAX_PORT_COUNT];

#if UNITY_EDITOR
    [SerializeField]
    private List<NodeLink> _inputLinks = new();

    [SerializeField]
    private List<NodeLink> _outputLinks = new();

    [SerializeField]
    private Rect _position;

    [SerializeField, ReadOnly]
    private SerializableType _nType = new();
#endif

    public BaseNode()
    {
#if UNITY_EDITOR
        GenerateID();
#endif
    }

#if UNITY_EDITOR
    private void GenerateID()
    {
        if (_uniqueId == 0)
        {
            System.Random rnd = new();
            _uniqueId = rnd.Next();
        }
    }

    public List<NodeLink> GetNextFlowLinks()
    {
        List<NodeLink> flows = new();
        foreach (NodeLink link in _outputLinks)
        {
            if (GetPort(link.FromPortId).DataType == null)
            {
                flows.Add(link);
            }
        }

        return flows;
    }

    public bool ShouldBreakIntoParts()
    {
        if (ForceBreakIntoParts)
        {
            return true;
        }

        int counter = 0;
        foreach (INodePort port in _ports)
        {
            if (port != null && port.Direction == PortDirection.Output && port.DataType == null)
            {
                counter++;
            }
        }
        return counter > 1;
    }

    public INodePort GetPortFromName(string name)
    {
        foreach (INodePort port in _ports)
        {
            if (port != null && port.Name == name)
                return port;
        }
        return null;
    }

    public void ResetDataLinks()
    {
        Array.Resize(ref _dataLinks, 10);
    }

    public void ResetPartLinks()
    {
        Array.Resize(ref _partLinks, 0);
    }

    public void PushBackPartLink(GraphPartLink partLink)
    {
        Array.Resize(ref _partLinks, _partLinks.Length + 1);
        _partLinks[_partLinks.Length - 1] = partLink;
    }
#endif

    public virtual Awaitable Execute(IGraphContext ctx, GraphBlackboard board, GraphPartData data)
    {
        return GraphAwaitable.CompletedAwaitable;
    }

    public virtual int GetPartsOnPort(
        int portID,
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData data,
        ref bool async
    )
    {
        return 1;
    }

    public virtual INodeData RegisterPartData(INodeData entryNodeData)
    {
        return null;
    }

    public INodePort GetPort(int uniqueId)
    {
        return _ports[uniqueId];
    }

    public TValue GetDataFromPort<TValue>(
        int id,
        IGraphContext ctx,
        GraphBlackboard board,
        GraphPartData data
    )
    {
        GraphNodeDataLink dataLink = _dataLinks[id];
        if (dataLink.FromNode == null)
            return default(TValue);

        INodePort port = dataLink.FromNode.GetPort(dataLink.FromPortId);
        NodeOutputDataPort<TValue> dataPort = (NodeOutputDataPort<TValue>)port;
        return dataPort.GetValue(ctx, board, data);
    }

    public void AddPort(INodePort port)
    {
        _ports[port.Id] = port;
    }

    public int UniqueId => _uniqueId;
    public int RuntimeId
    {
        get { return _runtimeId; }
        set { _runtimeId = value; }
    }
    public GraphNodeDataLink[] DataLinks => _dataLinks;
    public GraphPartLink[] PartLinks => _partLinks;
    public INodePort[] Ports => _ports;

#if UNITY_EDITOR
    public virtual bool ForceBreakIntoParts => false;
    public List<NodeLink> InputLinks => _inputLinks;
    public List<NodeLink> OuputLinks => _outputLinks;
    public virtual Color Color
    {
        get { return Color.white; }
    }
    public Rect Position
    {
        get { return _position; }
        set { _position = value; }
    }
    public SerializableType NType => _nType;
#endif
}
