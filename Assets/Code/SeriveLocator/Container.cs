using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-2)]
public abstract class Container : MonoBehaviour
{
    private Dictionary<Type, IService> _services;
    
    public void Awake()
    {
        _services = new Dictionary<Type, IService>();
    }

    public void Register(IService service)
    {
        if (_services.ContainsKey(service.GetType()))
        {
            Debug.LogError("Service: " + service.GetType() + " already exists!");
            return;
        }
        _services.Add(service.GetType(), service);
    }
    
    public T Resolve<T>() where T:IService
    {
        return (T) _services[typeof(T)];
    }
}
