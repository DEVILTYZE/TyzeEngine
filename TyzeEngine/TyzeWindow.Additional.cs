using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

public partial class TyzeWindow
{
    private void LoadObjects(IReadOnlyList<IGameObject> objects)
    {
        var result = new List<float>();

        foreach (var obj in objects)
        {
            
        }
    }
}