﻿using System;

namespace TyzeEngine;

public readonly struct Uid : IEquatable<Uid>
{
    public uint Value { get; }

    public Uid() => Value = Generate();

    public static uint Generate()
    {
        var random = new Random();
        var part1 = random.Next((int)uint.MinValue, int.MaxValue);
        var part2 = random.Next((int)uint.MinValue, int.MaxValue);

        return (uint)(part1 + part2);
    }

    public bool Equals(Uid other) => Value == other.Value;

    public override bool Equals(object obj) => obj is Uid other && Equals(other);

    public override int GetHashCode() => (int)Value;

    public static bool operator ==(Uid left, Uid right) => left.Equals(right);

    public static bool operator !=(Uid left, Uid right) => !left.Equals(right);
    
}