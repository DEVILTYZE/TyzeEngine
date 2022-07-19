using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class Characteristic : ICharacteristic
{
    public float Value { get; }

    public abstract void ChangeValue(float value);
}