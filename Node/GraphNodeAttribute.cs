using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class GraphNodeAttribute : Attribute
{
    public string Name { get; }
    public string Category { get; }
    public string Description { get; }
    public Type ContextRestriction { get; set; } = null;

    public GraphNodeAttribute(string name, string category, string description = "")
    {
        Name = name;
        Category = category;
        Description = description;
    }
}
