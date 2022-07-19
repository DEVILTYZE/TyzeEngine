namespace TyzeEngine.Interfaces;

public interface ICharacteristic
{
    float Value { get; }

    void ChangeValue(float value);
}