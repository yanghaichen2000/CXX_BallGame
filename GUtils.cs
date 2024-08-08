using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUtils
{
    public static DateTime lastLogTime = DateTime.MinValue;

    public static void LogWithCD<T>(T content, float cd = 0.5f)
    {
        if ((DateTime.Now - lastLogTime).TotalSeconds > cd)
        {
            Debug.Log(content);
            lastLogTime = DateTime.Now;
        }
    }

    public static ref Vector3 EliminateY(ref Vector3 input)
    {
        input.y = 0.0f;
        return ref input;
    }

    public static int GetComputeGroupNum(int threadNum, int groupSize)
    {
        return (threadNum + groupSize) / groupSize;
    }

    public class PFL : IDisposable
    {
        public PFL(string name)
        {
            UnityEngine.Profiling.Profiler.BeginSample(name);
        }

        public void Dispose()
        {
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public static Vector3 Vector3Pow(Vector3 x, float y)
    {
        return new Vector3(MathF.Pow(x.x, y), MathF.Pow(x.y, y), MathF.Pow(x.z, y));
    }

    public static Vector3 Vector3Mul(Vector3 x, Vector3 y)
    {
        return new Vector3(x.x * y.x, x.y * y.y, x.z * y.z);
    }
}

