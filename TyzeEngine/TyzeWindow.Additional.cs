using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine;

public partial class TyzeWindow
{
    private double _time;
    private int _frames;
    private readonly string _title;
    
    private void LoadObjects(TriggeredEventArgs args)
    {
        var objects = GetObjects(args is not null && (bool)args.Data);
        var models = GetModels();
        const int stride = Constants.VertexStride + Constants.TextureStride + Constants.ColorStride;
        
        // Загрузка объекта.
        foreach (var obj in objects)
        {
            // Создание нового Array object для каждого игрового объекта.
            obj.ArrayObject = new ArrayObject();
            obj.ArrayObject.Enable();
            // Получение точек позиции объекта в пространстве, текстуры в пространстве и цвета в виде массива float
            // и получение массива uint для Element object.
            var vertices = models[obj.ModelId].GetVectorArray(obj);

            // Создание буферa для векторного представления.
            var buffer = new BufferObject(BufferTarget.ArrayBuffer);
            buffer.SetData(vertices.Item1, obj.DrawType);
            obj.ArrayObject.AttachBuffer(buffer, 0);
            
            // Получение индексов для трёх атрибутов (позиция, текстура и цвет).
            var position = _shader.GetAttributeLocation("aPosition");
            var texture = _shader.GetAttributeLocation("inTexture");
            var color = _shader.GetAttributeLocation("inColor");
            
            buffer.Enable();
            obj.ArrayObject.EnableAttribute(position, Constants.VertexLength, VertexAttribPointerType.Float, 
                stride, 0);
            obj.ArrayObject.EnableAttribute(texture, Constants.TextureLength, VertexAttribPointerType.Float, 
                stride, Constants.VertexStride);
            obj.ArrayObject.EnableAttribute(color, Constants.ColorLength, VertexAttribPointerType.Float, 
                stride, Constants.VertexStride + Constants.TextureStride);
            buffer.Disable();

            // Создание буфера для Element object.
            var indicesBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
            indicesBuffer.SetData(vertices.Item2, obj.DrawType);
            obj.ArrayObject.AttachBuffer(indicesBuffer, vertices.Item2.Length);
            
            obj.ArrayObject.Disable();
            // Связывание ресурсов для текущего объекта.
            obj.EnableResources(GetResources());
        }
    }

    private void DrawObjects()
    {
        Matrix4 GetMatrix(IGameObject obj)
        {
            var scale = Matrix4.Identity * Matrix4.CreateScale(obj.Size);
            var rotationX = scale * Matrix4.CreateRotationX(obj.Rotation.X);
            var rotationY = rotationX * Matrix4.CreateRotationY(obj.Rotation.Y);
            var rotationZ = rotationY * Matrix4.CreateRotationZ(obj.Rotation.Z);
            
            return rotationZ * Matrix4.CreateTranslation(obj.Position);
        }
        void SetMatrices(IGameObject obj)
        {
            var model = GetMatrix(obj);
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _view);
            _shader.SetMatrix4("projection", _projection);
        }
        
        var objects = GetObjects(false).ToList();

        foreach (var obj in objects)
        {
            obj.ArrayObject.Enable();
            obj.EnableResources(GetResources());
            SetMatrices(obj);
            obj.ArrayObject.Draw(DrawElementsType.UnsignedInt);
            obj.DisableResources(GetResources());
        }
        
        PhysicsGenerator.Collision2D(objects.ToList());
    }

    private void LoadScene(TriggeredEventArgs args) => _currentSceneIndex = (int)args.Data;

    private IEnumerable<IGameObject> GetObjects(bool unloaded)
    {
        var currentPlace = _scenes[_currentSceneIndex].CurrentPlace;

        if (!unloaded)
            return new[] { currentPlace }.Concat(currentPlace.NeighbourPlaces).SelectMany(place => place.Objects);

        var objects = LoadQueue.TakeObjects().ToArray();
        currentPlace.Objects.AddRange(objects);
        _scenes[_currentSceneIndex].LoadResources();

        return objects;
    }

    private Dictionary<Uid, IModel> GetModels() => _scenes[_currentSceneIndex].Models;
    
    private Dictionary<Uid, IResource> GetResources() => _scenes[_currentSceneIndex].Resources;

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

    private void InitializeAudio()
    {
        var attribute = 1;
        _device = ALC.OpenDevice(null);
        _context = ALC.CreateContext(_device, ref attribute);
    
        ALC.MakeContextCurrent(_context);
    
        var version = AL.Get(ALGetString.Version);
        var vendor = AL.Get(ALGetString.Vendor);
        var renderer = AL.Get(ALGetString.Renderer);
        Console.WriteLine(version + "\r\n" + vendor + "\r\n" + renderer);
    }
}