using System;
using System.Text.Json;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

/// <summary>
/// Структура уникального идентификатора.
/// </summary>
[Serializable]
// ReSharper disable once InconsistentNaming
public readonly struct UID : IEquatable<UID>, IComparable<UID>, ISaveable
{
    private static readonly Random Random = new();
    private static readonly UID DefaultUid = new(ulong.MinValue);
    private readonly ulong _value = 0;

    public static ref readonly UID Default => ref DefaultUid;

    public ulong Value
    {
        get => _value;
        init
        {
            _value = value;
            ShortValue = (short)(value % 10000);
        }
    }

    public short ShortValue { get; private init; } = 0;

    public UID() => Value = Generate();

    public UID(ulong value) => Value = value;

    public UID(long value) => Value = value < 0 ? long.MaxValue + (ulong)-value : (ulong)value;

    public UID(uint value) : this((ulong)value)
    {
    }

    public UID(int value) : this((long)value)
    {
    }

    public UID(ushort value) : this((ulong)value)
    {
    }

    public UID(short value) : this((long)value)
    {
    }

    public UID(byte value) : this((ulong)value)
    {
    }

    public UID(sbyte value) : this((long)value)
    {
    }

    public UID(string value)
    {
        var isParsed = long.TryParse(value, out var longValue);
        
        if (isParsed)
        {
            Value = longValue < 0 ? long.MaxValue + (ulong)-longValue : (ulong)longValue;
            return;
        }

        isParsed = ulong.TryParse(value, out var ulongValue);

        if (!isParsed) 
            throw new ArgumentException("Wrong number type.", nameof(value));
        
        Value = ulongValue;
    }

    public bool Equals(UID other) => Value == other.Value;

    public override bool Equals(object obj) => obj is UID other && Equals(other);

    public override int GetHashCode() => (int)(Value % int.MaxValue);

    public override string ToString() => Value.ToString();

    public static bool operator ==(UID left, UID right) => left.Equals(right);

    public static bool operator !=(UID left, UID right) => !left.Equals(right);

    public int CompareTo(UID other) => Value.CompareTo(other.Value);

    public byte[] Save() => JsonSerializer.SerializeToUtf8Bytes(this);

    public static UID GetByBytes(byte[] data) => JsonSerializer.Deserialize<UID>(data);

    private static ulong Generate()
    {
        var buffer = new byte[sizeof(long)];
        Random.NextBytes(buffer);

        return BitConverter.ToUInt64(buffer);
    }
}