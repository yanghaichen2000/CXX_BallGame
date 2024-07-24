using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancePool<T> where T : new()
{
    public Stack<T> data;

    public InstancePool()
    {
        data = new Stack<T>();
    }

    public T Get()
    {
        if (data.Count != 0)
        {
            return data.Pop();
        }
        else
        {
            return new T();
        }
    }

    public void Return(T instance)
    {
        data.Push(instance);
    }
}