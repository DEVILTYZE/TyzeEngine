using System;

namespace TyzeEngine.Physics;

public readonly struct TypeKey : IEquatable<TypeKey>, IComparable<TypeKey>, IComparable
{
    public Type Type1 { get; }
    public Type Type2 { get; }

    public TypeKey(Type type1, Type type2)
    {
        Type1 = type1;
        Type2 = type2;
    }
    
    public bool Equals(TypeKey other) => Type1 == other.Type1 && Type2 == other.Type2;

    public override bool Equals(object obj) => obj is TypeKey other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(Type1, Type2);

    public int CompareTo(TypeKey other) => Equals(other) ? 0 : 1;

    public int CompareTo(object obj)
    {
        if (ReferenceEquals(null, obj)) 
            return 1;
        
        return obj is TypeKey other 
            ? CompareTo(other) 
            : throw new ArgumentException($"Object must be of type {nameof(TypeKey)}");
    }

    public static bool operator ==(TypeKey left, TypeKey right) => left.Equals(right);

    public static bool operator !=(TypeKey left, TypeKey right) => !left.Equals(right);

    public override string ToString() => "[" + Type1.Name + ", " + Type2.Name + "]";
}