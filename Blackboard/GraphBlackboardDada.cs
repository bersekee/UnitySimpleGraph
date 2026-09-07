using System;
using System.Collections.Generic;

[Serializable]
public class GraphBlackboardData
{
    public List<GraphBlackboardVariableData> Variables = new();

    private int _nextId = 1;

    public GraphBlackboardVariableData AddVariable(string name, string typeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Variable name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(typeId))
            throw new ArgumentException("Type ID cannot be empty.", nameof(typeId));

        if (ContainsVariable(name))
            throw new InvalidOperationException($"A variable named '{name}' already exists.");

        int id = GenerateId();

        GraphBlackboardVariableData variable = new GraphBlackboardVariableData(id, name, typeId, "");

        Variables.Add(variable);

        return variable;
    }

    public bool Remove(GraphBlackboardKey key)
    {
        for (int i = 0; i < Variables.Count; i++)
        {
            if (Variables[i].Id != key.Id)
                continue;

            Variables.RemoveAt(i);
            return true;
        }

        return false;
    }

    public GraphBlackboardVariableData Get(GraphBlackboardKey key)
    {
        for (int i = 0; i < Variables.Count; i++)
        {
            if (Variables[i].Id == key.Id)
                return Variables[i];
        }

        return null;
    }

    public GraphBlackboardVariableData Get(string name)
    {
        for (int i = 0; i < Variables.Count; i++)
        {
            if (Variables[i].Name == name)
                return Variables[i];
        }

        return null;
    }

    public bool ContainsVariable(string name)
    {
        return Get(name) != null;
    }

    public bool Contains(GraphBlackboardKey key)
    {
        return Get(key) != null;
    }

    public void Rename(GraphBlackboardKey key, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Variable name cannot be empty.", nameof(newName));

        GraphBlackboardVariableData variable = Get(key);

        if (variable == null)
            return;

        if (variable.Name == newName)
            return;

        if (ContainsVariable(newName))
            throw new InvalidOperationException($"A variable named '{newName}' already exists.");

        variable.Name = newName;
    }

    public string GetName(GraphBlackboardKey key)
    {
        return Get(key)?.Name;
    }

    public string GetTypeId(GraphBlackboardKey key)
    {
        return Get(key)?.TypeId;
    }

    private int GenerateId()
    {
        while (Contains(new GraphBlackboardKey(_nextId)))
        {
            _nextId++;
        }

        return _nextId++;
    }

    public void RebuildNextId()
    {
        int highestId = 0;

        foreach (GraphBlackboardVariableData variable in Variables)
        {
            if (variable.Id > highestId)
                highestId = variable.Id;
        }

        _nextId = highestId + 1;
    }
}
