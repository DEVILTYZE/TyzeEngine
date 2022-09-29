namespace TyzeEngine.Physics;

public abstract class CollisionCallback
{
    public abstract void Invoke(CollisionEventArgs args);
}