namespace TyzeEngine.Interfaces;

public interface ICollisionCallback
{ 
    void Invoke(CollisionEventArgs args);
}