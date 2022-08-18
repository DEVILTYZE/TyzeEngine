using System;

namespace TyzeEngine;

public readonly struct Uid : IEquatable<Uid>, IComparable<Uid>
{
    public static readonly Uid Zero = new(uint.MinValue);
    
    public uint Value { get; }

    public Uid() => Value = Generate();

    public Uid(uint value) => Value = value;

    public Uid(int value) => Value = value < 0 ? Generate() : (uint)value;

    public bool Equals(Uid other) => Value == other.Value;

    public override bool Equals(object obj) => obj is Uid other && Equals(other);

    public override int GetHashCode() => (int)Value;

    public override string ToString() => Value.ToString();

    public static bool operator ==(Uid left, Uid right) => left.Equals(right);

    public static bool operator !=(Uid left, Uid right) => !left.Equals(right);

    private static uint Generate()
    {
        var random = new Random();
        var part1 = random.Next((int)uint.MinValue, int.MaxValue);
        var part2 = random.Next((int)uint.MinValue, int.MaxValue);

        return (uint)(part1 + part2);
    }

    public int CompareTo(Uid other) => Value.CompareTo(other.Value);
}