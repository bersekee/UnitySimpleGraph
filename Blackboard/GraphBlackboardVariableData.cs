using System;

[Serializable]
public class GraphBlackboardVariableData
{
    public int Id;
    public string Name;
    public string TypeId;
    public string Value;
    public GraphBlackboardVariableData() { }

    public GraphBlackboardVariableData(int id, string name, string typeId, string value)
    {
        Id = id;
        Name = name;
        TypeId = typeId;
        Value = value;
    }

    public GraphBlackboardKey Key => new GraphBlackboardKey(Id);
}
