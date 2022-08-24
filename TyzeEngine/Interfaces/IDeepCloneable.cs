namespace TyzeEngine.Interfaces;

public interface IDeepCloneable<out T>
{
    T Clone();
}