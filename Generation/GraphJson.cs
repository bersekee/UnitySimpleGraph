using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GraphJson
{
    public string Name;
    public string Version;
    public string GraphType;
    public List<VariableJson> Variables;
    public List<NodeJson> Nodes;
    public List<LinkJson> Links;
}

[Serializable]
public class VariableJson
{
    public string Name;
    public string Type;
    public string Value;
}

[Serializable]
public class NodeJson
{
    public string Id;
    public string Type;
    public Rect Position;
    public List<PropertyJson> Properties;
}

[Serializable]
public class PropertyJson
{
    public string Name;
    public string Value;
}

[Serializable]
public class LinkJson
{
    public int FromPort;
    public int FromNode;
    public int ToPort;
    public int ToNode;
}