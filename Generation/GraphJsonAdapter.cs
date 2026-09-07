#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class GraphJsonAssetAdapter
{
    public static string GetCurrentVersion()
    {
        return "1.0.0";
    }

    public static bool IsGraphAsset(UnityEngine.Object obj)
    {
        return obj is BaseGraph;
    }

    public static GraphJson BuildJson(BaseGraph graph)
    {
        if (graph == null)
            return null;

        GraphJson result = new GraphJson
        {
            Name = graph.name,
            Version = GetCurrentVersion(),
            GraphType = graph.GetType().Name
        };

        result.Variables = new List<VariableJson>();
        foreach (GraphBlackboardVariableData variable in graph.BlacboardData.Variables)
        {
            result.Variables.Add(
                new VariableJson
                {
                    Name = variable.Name,
                    Type = GraphBlackboardTypeRegistry.GetType(variable.TypeId).ToString(),
                    Value = variable.Value
                });
        }

        result.Nodes = new List<NodeJson>();
        foreach (BaseNode node in graph.Nodes)
        {
            FieldInfo[] fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            List<PropertyJson> properties = new();
            foreach (FieldInfo field in fields)
            {
                PropertyJson property = new PropertyJson();

                property.Name = field.Name;
                property.Value = field.GetValue(node).ToString();

                properties.Add(property);
            }

            NodeJson jsonNode = new NodeJson
            {
                Id = node.UniqueId.ToString(),
                Type = node.GetType().ToString(),
                Position = node.Position,
                Properties = properties
            };

            result.Nodes.Add(jsonNode);
        }

        result.Links = new List<LinkJson>();
        foreach (BaseNode node in graph.Nodes)
        {
            foreach (NodeLink link in node.OuputLinks)
            {
                result.Links.Add(
                    new LinkJson
                    {
                        FromNode = link.FromNodeId,
                        FromPort = link.FromPortId,
                        ToNode = link.ToNodeId,
                        ToPort = link.ToPortId
                    });
            }
        }

        return result;
    }
    public static BaseGraph CreateGraph(GraphJson json)
    {
        Type graphType = FindGraphType(json.GraphType);

        if (graphType == null)
        {
            throw new Exception($"Could not find graph type '{json.GraphType}'.");
        }

        if (!typeof(BaseGraph).IsAssignableFrom(graphType))
        {
            throw new Exception($"Type '{graphType.FullName}' does not inherit from BaseGraph.");
        }

        BaseGraph graph = ScriptableObject.CreateInstance(graphType) as BaseGraph;

        if (graph == null)
        {
            throw new Exception($"Could not create graph '{graphType.FullName}'.");
        }

        return graph;
    }

    public static void PopulateGraph(BaseGraph graph, GraphJson json)
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));

        if (json == null)
            throw new ArgumentNullException(nameof(json));

        graph.name = json.Name;

        ImportVariables(graph, json.Variables);

        Dictionary<string, BaseNode> nodes = ImportNodes(graph, json.Nodes);

        ImportLinks(graph, json.Links, nodes);
    }

    private static void ImportVariables(BaseGraph graph, List<VariableJson> variables)
    {
        if (variables == null)
            return;

        foreach (VariableJson variable in variables)
        {
            if (variable == null)
                continue;

            /*
             * TODO:
             *
             * Replace this with your actual variable creation.
             *
             * Example:
             *
             * graph.AddVariable(
             *     variable.Name,
             *     variable.Type,
             *     variable.Value);
             */
        }
    }
    private static Dictionary<string, BaseNode> ImportNodes(BaseGraph graph, List<NodeJson> nodes)
    {
        Dictionary<string, BaseNode> result = new();

        if (nodes == null)
            return result;

        foreach (NodeJson nodeJson in nodes)
        {
            if (nodeJson == null)
                continue;

            if (string.IsNullOrEmpty(nodeJson.Id))
            {
                Debug.LogError("Encountered node without an Id.");
                continue;
            }

            Type nodeType = FindNodeType(nodeJson.Type);

            if (nodeType == null)
            {
                Debug.LogError($"Could not find node type '{nodeJson.Type}'.");
                continue;
            }

            object nodeObject = Activator.CreateInstance(nodeType);
            BaseNode node = nodeObject as BaseNode;
            if (node == null)
            {
                Debug.LogError($"The node type doesn not inherit BaseNode '{nodeJson.Type}'.");
                continue;
            }
            
            node.Position = nodeJson.Position;

            graph.AddNode(node);

            result.Add(nodeJson.Id, node);

            ImportProperties(node, nodeJson.Properties);
        }

        return result;
    }

    private static void ImportProperties(BaseNode node, List<PropertyJson> properties)
    {
        if (node == null || properties == null)
        {
            return;
        }

        FieldInfo[] fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyJson property in properties)
        {
            if (property == null)
                continue;

            FieldInfo fieldInfo = fields.FirstOrDefault(x => x.Name == property.Name);
            if (fieldInfo == null)
                continue;

            fieldInfo.SetValue(node, property.Value);
        }
    }

    private static void ImportLinks(BaseGraph graph, List<LinkJson> links, Dictionary<string, BaseNode> nodes)
    {
        if (links == null)
            return;

        foreach (LinkJson link in links)
        {
            if (!nodes.TryGetValue(link.FromNode.ToString(), out BaseNode fromNode))
            {
                Debug.LogError($"Link references missing source node " + $"'{link.FromNode}'.");
                continue;
            }

            if (!nodes.TryGetValue(link.ToNode.ToString(), out BaseNode toNode))
            {
                Debug.LogError($"Link references missing destination node " + $"'{link.ToNode}'.");
                continue;
            }

            fromNode.OuputLinks.Add(new NodeLink { FromNodeId = fromNode.UniqueId, FromPortId = link.FromPort, ToNodeId = toNode.UniqueId, ToPortId = link.ToPort });

            toNode.InputLinks.Add(new NodeLink { FromNodeId = fromNode.UniqueId, FromPortId = link.FromPort, ToNodeId = toNode.UniqueId, ToPortId = link.ToPort });
        }
    }

    private static Type FindGraphType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return null;

        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);

            if (type != null && typeof(BaseGraph).IsAssignableFrom(type))
            {
                return type;
            }

            foreach (Type candidate in GetSafeTypes(assembly))
            {
                if (candidate.Name != typeName)
                    continue;

                if (typeof(BaseGraph).IsAssignableFrom(candidate))
                    return candidate;
            }
        }

        return null;
    }

    private static Type FindNodeType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return null;

        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);

            if (type != null)
                return type;

            foreach (Type candidate in GetSafeTypes(assembly))
            {
                if (candidate.Name == typeName)
                    return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<Type> GetSafeTypes(System.Reflection.Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch
        {
            return Array.Empty<Type>();
        }
    }
}

#endif