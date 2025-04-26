using System;
using System.Collections.Generic;

public class EventBus : IService
{
    private Dictionary<Type, List<object>> _events = new();
    private Dictionary<Type, List<Action>> _noArgsEvents = new();
    
    public void Subscribe<T>(Action<T> action)
    {
        Type t = typeof(T);
        if (_events.ContainsKey(t))
        {
            _events[t].Add(action);
        }
        else
        {
            _events.Add(t,new List<object>());
            _events[t].Add(action);
        }
    }
    
    public void Subscribe<T>(Action action)
    {
        Type t = typeof(T);
        if (_noArgsEvents.ContainsKey(t))
        {
            _noArgsEvents[t].Add(action);
        }
        else
        {
            _noArgsEvents.Add(t,new List<Action>());
            _noArgsEvents[t].Add(action);
        }
    }
    
    public void Unsubscribe<T>(Action<T> action)
    {
        Type t = typeof(T);
        if (_events.ContainsKey(t))
        {
            _events[t].Remove(action);
            if (_events[t].Count == 0)
            {
                _events.Remove(t);
            }
        }
    }

    public void Unsubscribe<T>(Action action) 
    {
        Type t = typeof(T);
        if (_noArgsEvents.ContainsKey(t))
        {
            _noArgsEvents[t].Remove(action);
            if (_noArgsEvents[t].Count == 0)
            {
                _noArgsEvents.Remove(t);
            }
        }
    }
    

    public void Fire<T>(T data)
    {
        Type t = typeof(T);
        if (_events.ContainsKey(t))
        {
            foreach (var action in _events[t])
            {
                ((Action<T>)action).Invoke(data);
                
            }
        }

        if (_noArgsEvents.ContainsKey(t))
        {
            foreach (var action in _noArgsEvents[t])
            {
                action.Invoke();
            }
        }
    }
    
    public void Fire<T>()
    {
        Type t = typeof(T);
        foreach (var action in _noArgsEvents[t])
        {
            action.Invoke();
        }
    }
}

