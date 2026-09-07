using System;
using System.Collections.Generic;
using UnityEngine;

public static class GraphBlackboardTypeRegistry
{
    public readonly struct Entry
    {
        public readonly string Id;
        public readonly Type Type;
        public readonly string Name;

        public Entry(string id, Type type, string name)
        {
            Id = id;
            Type = type;
            Name = name;
        }
    }

    private static readonly List<Entry> _entries = new();

    public static IReadOnlyList<Entry> Entries => _entries;

    static GraphBlackboardTypeRegistry()
    {
        Register<int>("System.Int32", "Integer");

        Register<float>("System.Single", "Float");

        Register<bool>("System.Boolean", "Boolean");

        Register<string>("System.String", "String");

        Register<Vector2>("Unity.Vector2", "Vector2");

        Register<Vector3>("Unity.Vector3", "Vector3");

        Register<Vector4>("Unity.Vector4", "Vector4");

        Register<Color>("Unity.Color", "Color");
    }

    public static void Register<T>(string id, string name)
    {
        Register(id, typeof(T), name);
    }

    public static void Register(string id, Type type, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Type ID cannot be empty.", nameof(id));

        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Type name cannot be empty.", nameof(name));

        if (Find(id).HasValue)
            return;

        _entries.Add(new Entry(id, type, name));
    }

    public static Entry? Find(string id)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].Id == id)
                return _entries[i];
        }

        return null;
    }

    public static Entry? Find(Type type)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].Type == type)
                return _entries[i];
        }

        return null;
    }

    public static Type GetType(string id)
    {
        Entry? entry = Find(id);

        return entry.HasValue ? entry.Value.Type : null;
    }
}
