using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

public partial class TyzeWindow
{
    private double _time;
    private int _frames;
    private readonly string _title;
    
    private void LoadObjects(EventTriggeredArgs args = null)
    {
        _scenes[_currentSceneIndex].Start();
        var objects = GetObjects();

        // Загрузка объекта.
        foreach (var obj in objects)
        {
            // Создание нового Array object для каждого игрового объекта.
            obj.ArrayObject = new ArrayObject();
            obj.ArrayObject.Enable();
            // Получение точек позиции объекта в пространстве в виде массива float и получение массива uint для Element object.
            var vectorObject = obj.Model.GetVectorArray();

            // Создание буферов для координатного и визуального представления.
            var coordinateBuffer = new BufferObject(BufferTarget.ArrayBuffer);
            var visualBuffer = new BufferObject(BufferTarget.ArrayBuffer);

            coordinateBuffer.SetData(vectorObject.Vertices, obj.DrawType);
            visualBuffer.SetData(vectorObject.VisualArray, obj.DrawType);

            obj.ArrayObject.AttachBuffer(coordinateBuffer, 0);
            obj.ArrayObject.AttachBuffer(visualBuffer, 0);

            coordinateBuffer.Enable();
            // Получение индексов для трёх атрибутов (позиция, текстура и цвет).
            var position = _shader.GetAttributeLocation("aPosition");
            var texture = _shader.GetAttributeLocation("inTexture");
            var color = _shader.GetAttributeLocation("inColor");
            obj.ArrayObject.EnableAttribute(position, ConstHelper.VertexLength, VertexAttribPointerType.Float, 
                ConstHelper.VertexStride, 0);
            coordinateBuffer.Disable();
            
            visualBuffer.Enable();
            obj.ArrayObject.EnableAttribute(texture, ConstHelper.TextureLength, VertexAttribPointerType.Float, 
                ConstHelper.TextureStride + ConstHelper.ColorStride, 0);
            obj.ArrayObject.EnableAttribute(color, ConstHelper.ColorLength, VertexAttribPointerType.Float, 
                ConstHelper.TextureStride + ConstHelper.ColorStride, ConstHelper.TextureStride);
            visualBuffer.Disable();

            // Создание буфера для Element object.
            var indicesBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
            indicesBuffer.SetData(vectorObject.Indices, obj.DrawType);
            obj.ArrayObject.AttachBuffer(indicesBuffer, vectorObject.Indices.Length);
            
            obj.ArrayObject.Disable();
            // Связывание ресурсов для текущего объекта.
            obj.EnableResources();
        }
    }

    private void DrawObjects()
    {
        var objects = GetObjects();

        foreach (var obj in objects)
        {
            obj.EnableResources();
            obj.ArrayObject.Draw(DrawElementsType.UnsignedInt);
        }
    }

    private void UseShader(bool isEnable)
    {
        if (isEnable)
            _shader.Enable();
        else
            _shader.Disable();
        
    }

    private void LoadScene(EventTriggeredArgs args) => _currentSceneIndex = (int)args.Data;

    private IEnumerable<IGameObject> GetObjects()
    {
        var currentPlace = _scenes[_currentSceneIndex].CurrentPlace;

        return new[] { currentPlace }.Concat(currentPlace.NeighbourPlaces).SelectMany(place => place.Objects);
    }

    private void ShowFps(double time)
    {
        _time += time;
        ++_frames;

        if (_time < 1.0) 
            return;
        
        Title = _title + " / FPS: " + _frames;
        _time = 0.0;
        _frames = 0;
    }
    
}