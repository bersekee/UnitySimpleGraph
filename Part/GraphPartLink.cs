using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GraphPartLink
{
    [SerializeField]
    private int _port;

    [SerializeField]
    private GraphPart _part;

    public GraphPartLink(int port, GraphPart part)
    {
        _port = port;
        _part = part;
    }

    public int Port => _port;
    public GraphPart Part => _part;
}
