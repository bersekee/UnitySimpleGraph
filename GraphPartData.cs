using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

// TODO prevent allocation
[Serializable]
public abstract class INodeData
{
    public abstract object Clone();
}

[Serializable]
public class GraphPartData : IDisposable
{
    private INodeData[] _nodeData;
    private int _count;
    private int _partIndex;
    private bool _async;

    public void Init(BaseGraph graph, int partIndex, bool async)
    {
        _count = graph.Nodes.Count;
        _partIndex = partIndex;
        _async = async;
        _nodeData = ArrayPool<INodeData>.Shared.Rent(_count);
    }

    public void CreateData(BaseGraph graph, INodeData entryNodeData)
    {
        foreach (BaseNode node in graph.Nodes)
        {
            _nodeData[node.RuntimeId] = node.RegisterPartData(entryNodeData);
        }
    }

    public T Get<T>(BaseNode node)
        where T : INodeData
    {
        return (T)_nodeData[node.RuntimeId];
    }

    public void Copy(GraphPartData other)
    {
        for (int i = 0; i < _count; i++)
        {
            if (other._nodeData[i] != null)
            {
                _nodeData[i] = (INodeData)other._nodeData[i].Clone();
            }
        }
    }

    public void Dispose()
    {
        if (_nodeData != null)
        {
            ArrayPool<INodeData>.Shared.Return(_nodeData, clearArray: false);
            _nodeData = null;
        }
    }

    public int PartIndex => _partIndex;
    public bool Async => _async;
}
