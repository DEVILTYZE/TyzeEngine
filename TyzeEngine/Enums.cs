using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TyzeEngine;

public enum ArrayType
{
    Unknown = 0,
    OneDimension = 1,
    TwoDimensions = 2,
    ThreeDimensions = 3,
    FourDimensions = 4,
}

public enum Visibility
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

public enum KeyCode
{
    #region SystemKeys

    Escape = Keys.Escape,
    F1 = Keys.F1,
    F2 = Keys.F2,
    F3 = Keys.F3,
    F4 = Keys.F4,
    F5 = Keys.F5,
    F6 = Keys.F6,
    F7 = Keys.F7,
    F8 = Keys.F8,
    F9 = Keys.F9,
    F10 = Keys.F10,
    F11 = Keys.F11,
    F12 = Keys.F12,
    Tilda = Keys.GraveAccent,
    Minus = Keys.Minus,
    Plus = Keys.Equal,
    Tab = Keys.Tab,
    Capslock = Keys.CapsLock,
    Backspace = Keys.Backspace,
    Space = Keys.Space,
    Enter = Keys.Enter,
    LShift = Keys.LeftShift,
    LCtrl = Keys.LeftControl,
    LAlt = Keys.LeftAlt,
    RShift = Keys.RightShift,
    RCtrl = Keys.RightControl,
    RAlt = Keys.RightAlt,

    #endregion

    #region RightPartKeys

    Insert = Keys.Insert,
    Delete = Keys.Delete,
    Home = Keys.Home,
    End = Keys.End,
    PageUp = Keys.PageUp,
    PageDown = Keys.PageDown,
    Up = Keys.Up,
    Down = Keys.Down,
    Left = Keys.Left,
    Right = Keys.Right,

    #endregion

    #region NumPadKeys

    NumPadSlash = Keys.KeyPadDivide,
    NumPadMultiply = Keys.KeyPadMultiply,
    NumPadMinus = Keys.KeyPadSubtract,
    NumPadPlus = Keys.KeyPadAdd,
    NumPadEnter = Keys.KeyPadEnter,
    NumPadDot = Keys.KeyPadDecimal,
    NumPadNum0 = Keys.KeyPad0,
    NumPadNum1 = Keys.KeyPad1,
    NumPadNum2 = Keys.KeyPad2,
    NumPadNum3 = Keys.KeyPad3,
    NumPadNum4 = Keys.KeyPad4,
    NumPadNum5 = Keys.KeyPad5,
    NumPadNum6 = Keys.KeyPad6,
    NumPadNum7 = Keys.KeyPad7,
    NumPadNum8 = Keys.KeyPad8,
    NumPadNum9 = Keys.KeyPad9,

    #endregion

    #region NumberKeys

    Num0 = Keys.D0,
    Num1 = Keys.D1,
    Num2 = Keys.D2,
    Num3 = Keys.D3,
    Num4 = Keys.D4,
    Num5 = Keys.D5,
    Num6 = Keys.D6,
    Num7 = Keys.D7,
    Num8 = Keys.D8,
    Num9 = Keys.D9,

    #endregion

    #region WASD

    W = Keys.W,
    D = Keys.D,
    A = Keys.A,
    S = Keys.S,

    #endregion

    #region MainKeys

    Q = Keys.Q,
    E = Keys.E,
    R = Keys.R,
    T = Keys.T,
    Y = Keys.Y,
    U = Keys.U,
    I = Keys.I,
    O = Keys.O,
    P = Keys.P,
    F = Keys.F,
    G = Keys.G,
    H = Keys.H,
    J = Keys.J,
    K = Keys.K, 
    L = Keys.L,
    Z = Keys.Z,
    X = Keys.X,
    C = Keys.C,
    V = Keys.V,
    B = Keys.B,
    N = Keys.N,
    M = Keys.M,
    LBracket = Keys.LeftBracket,
    RBracket = Keys.RightBracket,
    Semicolon = Keys.Semicolon,
    Quote = Keys.Apostrophe,
    Comma = Keys.Comma,
    Dot = Keys.Period,
    Slash = Keys.Slash,

    #endregion
}

public enum ButtonCode
{
    Left = MouseButton.Left,
    Right = MouseButton.Right,
    Middle = MouseButton.Middle,
    AdditionalButton1 = MouseButton.Button4,
    AdditionalButton2 = MouseButton.Button5,
    AdditionalButton3 = MouseButton.Button6,
    AdditionalButton4 = MouseButton.Button7,
    AdditionalButton5 = MouseButton.Button8
}

internal enum GameAssetType
{
    Scene = 0,
    Place = 1,
    GameObject = 2,
    Script = 3,
    Trigger = 4,
    Resource = 5,
    Model = 6
}
