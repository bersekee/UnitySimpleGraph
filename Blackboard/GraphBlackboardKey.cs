using System;

[Serializable]
public struct GraphBlackboardKey : IEquatable<GraphBlackboardKey>
{
    public int Id;

    public GraphBlackboardKey(int id)
    {
        Id = id;
    }

    public bool IsValid => Id != 0;

    public bool Equals(GraphBlackboardKey other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        return obj is GraphBlackboardKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public static bool operator ==(GraphBlackboardKey left, GraphBlackboardKey right)
    {
        return left.Id == right.Id;
    }

    public static bool operator !=(GraphBlackboardKey left, GraphBlackboardKey right)
    {
        return left.Id != right.Id;
    }

    public override string ToString()
    {
        return Id.ToString();
    }
}
