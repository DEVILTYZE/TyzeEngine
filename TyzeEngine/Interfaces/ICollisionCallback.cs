namespace TyzeEngine.Interfaces;

public interface ICollisionCallback
{ 
    void Invoke(Manifold args);
}