using System;
using System.Collections.Generic;

public class GraphBlackboard
{
    private readonly Dictionary<int, object> _values = new();

    public void Set<T>(GraphBlackboardKey key, T value)
    {
        _values[key.Id] = value;
    }

    public void Set(GraphBlackboardKey key, object value)
    {
        _values[key.Id] = value;
    }

    public bool TryGet<T>(GraphBlackboardKey key, out T value)
    {
        if (_values.TryGetValue(key.Id, out object obj) && obj is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public T Get<T>(GraphBlackboardKey key, T fallback = default)
    {
        return TryGet(key, out T value) ? value : fallback;
    }

    public bool Has(GraphBlackboardKey key)
    {
        return _values.ContainsKey(key.Id);
    }

    public void Remove(GraphBlackboardKey key)
    {
        _values.Remove(key.Id);
    }

    public void Clear()
    {
        _values.Clear();
    }

    public void SetDefaultValues(GraphBlackboardData definition)
    {
        _values.Clear();

        if (definition == null)
            return;

        foreach (GraphBlackboardVariableData variable in definition.Variables)
        {
            Type type = GraphBlackboardTypeRegistry.GetType(variable.TypeId);

            if (type == null)
                continue;

            _values[variable.Id] = CreateDefaultValue(type);
        }
    }

    private static object CreateDefaultValue(Type type)
    {
        if (type == typeof(string))
            return string.Empty;

        if (type.IsValueType)
            return Activator.CreateInstance(type);

        return null;
    }
}
