using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GraphPart
{
    [SerializeReference]
    private List<BaseNode> _nodes = new();

    public List<BaseNode> Nodes => _nodes;

    public BaseNode EntryNode => _nodes[0];
}
