using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public abstract class BaseGraph : UniqueScriptableObject
{
    [SerializeReference]
    private List<BaseNode> _nodes = new();

    [SerializeField]
    private List<GraphPart> _graphParts = new();

    [SerializeReference]
    private List<GraphPart> _entryParts = new();

    [SerializeField]
    private GraphBlackboardData _blackboardData = new();

#if UNITY_EDITOR
    [SerializeField]
    private SerializableDictionary<int, BaseNode> _idToNodeMap = new();
#endif

    public override void OnAfterDeserialize()
    {
        base.OnAfterDeserialize();
#if UNITY_EDITOR
        RemoveInvalidNodes();

        FillIdToNodeMap();

        RemoveInvalidLinks();
#endif
    }

#if UNITY_EDITOR
    public override void OnBeforeSerialize()
    {
        base.OnBeforeSerialize();

        if (
            !EditorApplication.isPlaying
            && !EditorApplication.isUpdating
            && !EditorApplication.isCompiling
        )
        {
            CompileGraph();
        }
    }

    public void CompileGraph()
    {
        GraphCompiler compiler = new(this);
        compiler.Compile();
    }

    private void RemoveInvalidNodes()
    {
        _nodes.RemoveAll(n => n == null);
    }

    private void RemoveInvalidLinks()
    {
        foreach (BaseNode node in _nodes)
        {
            for (int i = node.OuputLinks.Count - 1; i >= 0; i--)
            {
                NodeLink link = node.OuputLinks[i];
                if (!_idToNodeMap.TryGetValue(link.FromNodeId, out BaseNode n1))
                {
                    node.OuputLinks.RemoveAt(i);
                }
                else if (!_idToNodeMap.TryGetValue(link.ToNodeId, out BaseNode n2))
                {
                    node.OuputLinks.RemoveAt(i);
                }
            }

            for (int i = node.InputLinks.Count - 1; i >= 0; i--)
            {
                NodeLink link = node.InputLinks[i];
                if (!_idToNodeMap.TryGetValue(link.FromNodeId, out BaseNode n1))
                {
                    node.InputLinks.RemoveAt(i);
                }
                else if (!_idToNodeMap.TryGetValue(link.ToNodeId, out BaseNode n2))
                {
                    node.InputLinks.RemoveAt(i);
                }
            }
        }
    }

    private void FillIdToNodeMap()
    {
        _idToNodeMap.Clear();

        foreach (BaseNode node in _nodes)
        {
            if (!_idToNodeMap.TryAdd(node.UniqueId, node))
            {
                Debug.LogError($"Node ID '{node.UniqueId}' collision detected for {node}.");
            }
        }
    }

    public void AddNode(BaseNode node)
    {
        _nodes.Add(node);
        _idToNodeMap.TryAdd(node.UniqueId, node);
    }

    public BaseNode FindNode(int id)
    {
        BaseNode node = null;
        _idToNodeMap.TryGetValue(id, out node);
        return node;
    }

    public virtual List<string> GetCategories()
    {
        return new List<string>();
    }

    public virtual Type GetDefaultEntryNode()
    {
        return typeof(EntryNode);
    }
#endif

    public List<BaseNode> Nodes => _nodes;
    public List<GraphPart> GraphParts => _graphParts;
    public List<GraphPart> EntryParts => _entryParts;
    public GraphBlackboardData BlacboardData => _blackboardData;

#if UNITY_EDITOR
    public SerializableDictionary<int, BaseNode> IdToNodeMap => _idToNodeMap;
#endif
}
