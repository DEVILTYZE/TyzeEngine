namespace TyzeEngine;

public enum ArrayType
{
    Unknown = 0,
    OneDimension = 1,
    TwoDimensions = 2,
    ThreeDimensions = 3,
    FourDimensions = 4,
}

public enum VisibilityType
{
    Visible = 0,
    Hidden = 1,
    Collapsed = 2
}

public enum BodyVisualType
{
    Color = 0,
    Texture = 1,
    ColorAndTexture = 2
}

public enum RunMode
{
    Debug = 0,
    Release = 1
}

public enum SaveStatus
{
    Unknown = 0,
    Succeed = 1,
    Error = 2
}

public enum GameAssetType
{
    Scene = 0,
    Place = 1,
    GameObject = 2,
    Script = 3,
    Trigger = 4,
    Resource = 5,
    Model = 6
}
