using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

public static class GraphRegistryGenerator
{
    [MenuItem("Assets/Stack/Graph/Generate Registry", false, 1)]
    private static void GenerateRegistry()
    {
        BaseGraph graph = Selection.activeObject as BaseGraph;
        Generate(graph);
    }

    [MenuItem("Assets/Stack/Graph/Generate Registry", true)]
    private static bool ValidateGenerateRegistry()
    {
        UnityEngine.Object selected = Selection.activeObject;

        return selected != null && GraphJsonAssetAdapter.IsGraphAsset(selected);
    }

    public static void Generate(BaseGraph graph)
    {
        GraphDefintionJson registry = new GraphDefintionJson();

        registry.Version = GraphJsonAssetAdapter.GetCurrentVersion();
        registry.GraphType = graph.GetType().ToString();

        List<string> categories = graph.GetCategories();

        foreach (Type nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())
        {
            if (nodeType.IsAbstract)
                continue;

            string category = GetCategoryName(nodeType);
            if (!categories.Contains(category))
                continue;

            BaseNode node = (BaseNode)Activator.CreateInstance(nodeType);

            registry.Nodes.Add(CreateNodeDefinition(node));
        }

        Directory.CreateDirectory("Assets/Graph");

        string json = JsonUtility.ToJson(registry, true);

        File.WriteAllText($"Assets/Graph/{graph.GetType().ToString()}Registry.json", json);

        AssetDatabase.Refresh();

        Debug.Log($"Generated {registry.Nodes.Count} node definitions.");
    }

    static NodeDefinitionJson CreateNodeDefinition(BaseNode node)
    {
        NodeDefinitionJson definition = new NodeDefinitionJson();

        definition.Type = node.GetType().Name;

        string name = Regex.Replace(node.GetType().Name, "(?<!^)([A-Z])", " $1");
        GraphNodeAttribute attribute = node.GetType().GetCustomAttribute<GraphNodeAttribute>();
        if (attribute != null)
        {
            definition.DisplayName = attribute.Name;
            definition.Description = attribute.Description;
            definition.Category = attribute.Category;
        }
        else
        {
            definition.DisplayName = "";
            definition.Description = "";
            definition.Category = "";
        }

        Type type = node.GetType();
        FieldInfo fieldInfo = type.GetField("Category", BindingFlags.Public | BindingFlags.Static);
        if (fieldInfo != null)
        {
            object value = fieldInfo.GetValue(null);
            definition.Category = (string)value;
        }

        foreach (INodePort port in node.Ports)
        {
            if (port != null)
            {
                if (port.Direction == PortDirection.Input)
                {
                    definition.Inputs.Add(CreatePort(port));
                }
                else
                {
                    definition.Outputs.Add(CreatePort(port));
                }
            }
        }

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            PropertyDefinitionJson property = new PropertyDefinitionJson();

            property.Name = field.Name;
            property.Type = field.FieldType.Name;

            object value = field.GetValue(node);

            property.DefaultValue = value != null ? value.ToString() : "";

            definition.Properties.Add(property);
        }

        return definition;
    }

    static PortDefinitionJson CreatePort(INodePort port)
    {
        return new PortDefinitionJson
        {
            Id = port.Id,
            Name = port.Name,
            Type = port.DataType == null ? "Null" : port.DataType.FullName,
            Capacity = port.Capacity.ToString(),
            Kind = port.DataType == null ? "Flow" : "Value",
        };
    }

    static string GetCategoryName(Type type)
    {
        GraphNodeAttribute attribute = type.GetCustomAttribute<GraphNodeAttribute>();
        if (attribute != null)
        {
            return attribute.Category;
        }

        return "Uncategorized";
    }
}

[Serializable]
public class GraphDefintionJson
{
    public string Version;
    public string GraphType;
    public List<NodeDefinitionJson> Nodes = new();
}

[Serializable]
public class NodeDefinitionJson
{
    public string Type;
    public string DisplayName;
    public string Description;
    public string Category;

    public List<PortDefinitionJson> Inputs = new();
    public List<PortDefinitionJson> Outputs = new();
    public List<PropertyDefinitionJson> Properties = new();
}

[Serializable]
public class PropertyDefinitionJson
{
    public string Name;
    public string Type;
    public string DefaultValue;
}

[Serializable]
public class PortDefinitionJson
{
    public int Id;
    public string Name;
    public string Type;

    public string Kind;
    public string Capacity;
}
