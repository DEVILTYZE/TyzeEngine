namespace TyzeEngine.Interfaces;

public interface IDeepCloneable<T> where T : class
{
    T Clone(T obj = null);
}