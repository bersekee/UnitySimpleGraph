using System;
using UnityEngine;

[Serializable]
public class SerializableType : ISerializationCallbackReceiver
{
    public Type Type { get; set; }

    [SerializeField]
    private string assemblyQualifiedName;

    public void OnBeforeSerialize()
    {
        if (Type != null)
            assemblyQualifiedName = Type.AssemblyQualifiedName;
    }

    public void OnAfterDeserialize()
    {
        if (!string.IsNullOrEmpty(assemblyQualifiedName))
            Type = Type.GetType(assemblyQualifiedName);
    }
}
