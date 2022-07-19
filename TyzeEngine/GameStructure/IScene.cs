using System.Collections.Generic;

namespace TyzeEngine.GameStructure;

public interface IScene
{
    bool LoadError { get; }
    ILighting Lighting { get; }
    IPlace CurrentPlace { get; }

    void LoadPlace(int id);
    void Start();
}