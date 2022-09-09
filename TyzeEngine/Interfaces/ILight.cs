namespace TyzeEngine.Interfaces;

public interface ILight
{
    float Ambient { get; }
    float Specular { get; }
    float Shininess { get; }
}