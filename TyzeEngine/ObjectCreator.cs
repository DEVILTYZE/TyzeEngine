using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

public class ObjectCreator<T> where T : class
{
    protected T Object;

    public virtual ObjectCreator<T> Create(string className, object[] args = null)
    {
        var assembly = Assembly.GetEntryAssembly();

        if (assembly is null)
            throw new NullReferenceException("Entry assembly is null.");
        
        var type = assembly.GetType(className) ?? Type.GetType(className);
        Object = Create(type, args);

        return this;
    }

    public virtual ObjectCreator<T> Create(object[] args = null)
    {
        Object = Create(typeof(T), args);

        return this;
    }

    public virtual ObjectCreator<T> SetProperty(string propertyName, object value)
    {
        SetProperty(Object, propertyName, value);
        
        return this;
    }

    public virtual void SetProperty(object obj, string propertyName, object value)
    {
        var names = propertyName.Split('.');
        var property = typeof(T).GetProperty(names[0]);
        var currentObj = obj;
        
        for (var i = 1; i < names.Length; ++i)
        {
            if (property is null)
                throw new NullReferenceException($"Property {names[i - 1]} is null.");

            currentObj = property.GetValue(currentObj, null);
            property = property.PropertyType.GetProperty(names[i]);
        }

        if (currentObj is null)
            throw new NullReferenceException("Object is null.");
        if (property is null)
            throw new NullReferenceException($"Property {names[^1]} is null.");
        if (!property.CanWrite)
            throw new Exception("Property isn't writable.");
        if (property.PropertyType is null || property.PropertyType != value.GetType() && value.GetType().GetInterfaces()
                .All(interfaceType => interfaceType != property.PropertyType))
            throw new Exception("Value has wrong type.");
        
        property.SetValue(currentObj, value);
    }

    public virtual T GetObject()
    {
        if (Object is null)
            throw new NullReferenceException("Object is null.");
        
        var obj = Object;
        Object = null;

        return obj;
    }

    public virtual T2 Clone<T2>(T2 obj) where T2 : class
    {
        var interfaceType = obj.GetType().GetInterface(typeof(IDeepCloneable<T2>).Name);
        
        if (interfaceType is null)
            throw new NullReferenceException("Object hasn't the IDeepCloneable interface.");

        var method = interfaceType.GetMethod("Clone");

        if (method is null)
            throw new NullReferenceException("Object hasn't the Clone method.");
        
        return (T2)method.Invoke(obj, new object[] { null });
    }

    private static bool IsThisConstructor(MethodBase constructor, IReadOnlyList<object> args)
    {
        var parameters = constructor.GetParameters().ToArray();

        return parameters.Where((parameter, i) => parameter.ParameterType == args[i].GetType()).Any();
    }

    private static T Create(Type type, object[] args)
    {
        if (type is null)
            throw new NullReferenceException("Type is null.");
        if (type.Attributes == TypeAttributes.Abstract)
            throw new Exception("Abstract class can't have instance.");

        var constructors = type.GetConstructors();
        ConstructorInfo ctr = null;

        if (args is null)
            ctr = constructors.FirstOrDefault(localCtr => !localCtr.CustomAttributes.Any());
        else foreach (var constructor in constructors) 
            if (IsThisConstructor(constructor, args)) 
                ctr = constructor;

        if (ctr is null)
            throw new NullReferenceException("Constructor is not found.");
        if (ctr.Attributes == MethodAttributes.Private)
            throw new Exception("Constructor is private.");

        return (T)ctr.Invoke(args);
    }
}
