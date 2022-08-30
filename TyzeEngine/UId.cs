﻿using System;
using System.Text.Json;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

[Serializable]
public readonly struct UId : IEquatable<UId>, IComparable<UId>, ISaveable
{
    private static readonly Random Random = new();
    private static readonly UId DefaultUid = new(ulong.MinValue);

    public static ref readonly UId Default => ref DefaultUid;
    public ulong Value { get; }

    public UId() => Value = Generate();

    public UId(ulong value) => Value = value;

    public UId(long value) => Value = value < 0 ? long.MaxValue + (ulong)-value : (ulong)value;

    public UId(uint value) : this((ulong)value)
    {
    }

    public UId(int value) : this((long)value)
    {
    }

    public UId(ushort value) : this((ulong)value)
    {
    }

    public UId(short value) : this((long)value)
    {
    }

    public UId(byte value) : this((ulong)value)
    {
    }

    public UId(sbyte value) : this((long)value)
    {
    }

    public UId(string value)
    {
        var isParsed = long.TryParse(value, out var longValue);
        
        if (isParsed)
        {
            Value = longValue < 0 ? long.MaxValue + (ulong)-longValue : (ulong)longValue;
            return;
        }

        isParsed = ulong.TryParse(value, out var ulongValue);
        
        if (isParsed)
            Value = ulongValue;

        throw new ArgumentException("Wrong number type.", nameof(value));
    }

    public bool Equals(UId other) => Value == other.Value;

    public override bool Equals(object obj) => obj is UId other && Equals(other);

    public override int GetHashCode() => (int)Value;

    public override string ToString() => Value.ToString();

    public static bool operator ==(UId left, UId right) => left.Equals(right);

    public static bool operator !=(UId left, UId right) => !left.Equals(right);

    public int CompareTo(UId other) => Value.CompareTo(other.Value);

    public byte[] Save() => JsonSerializer.SerializeToUtf8Bytes(this);

    public static UId GetByBytes(byte[] data) => JsonSerializer.Deserialize<UId>(data);

    private static ulong Generate()
    {
        var buffer = new byte[8];
        Random.NextBytes(buffer);

        return BitConverter.ToUInt64(buffer);
    }
}