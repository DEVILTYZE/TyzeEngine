namespace TyzeEngine.Interfaces;

public interface ILogger<in T>
{
    void Log(T data);

    void LogSync(T data);
}