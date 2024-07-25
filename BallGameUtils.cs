using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGameUtils
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

    public class Profiler : IDisposable
    {
        public Profiler(string name)
        {
            UnityEngine.Profiling.Profiler.BeginSample(name);
        }

        public void Dispose()
        {
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}