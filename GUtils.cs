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
        return (threadNum + groupSize - 1) / groupSize;
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

    public static Vector3 Pow(Vector3 x, float y)
    {
        return new Vector3(MathF.Pow(x.x, y), MathF.Pow(x.y, y), MathF.Pow(x.z, y));
    }

    public static Vector3 Mul(Vector3 x, Vector3 y)
    {
        return new Vector3(x.x * y.x, x.y * y.y, x.z * y.z);
    }

    public static Vector3 Lerp(Vector3 from, Vector3 to, float k)
    {
        k = Math.Clamp(k, 0.0f, 1.0f);
        return from * (1.0f - k) + to * k;
    }

    public static Vector3 Clamp(Vector3 vec, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(vec.x, min.x, max.x),
            Mathf.Clamp(vec.y, min.y, max.y),
            Mathf.Clamp(vec.z, min.z, max.z)
        );
    }

    private static float SRGBToLinear(float val)
    {
        if (val <= 0.04045f)
        {
            return val / 12.92f;
        }
        else
        {
            return Mathf.Pow((val + 0.055f) / 1.055f, 2.4f);
        }
    }

    public static Vector3 SRGBColorToLinearVector3(Color color)
    {
        Vector3 ret = new Vector3();
        ret.x = SRGBToLinear(color.r);
        ret.y = SRGBToLinear(color.g);
        ret.z = SRGBToLinear(color.b);
        return ret;
    }

    public static uint SRGBColorToLinearUInt(Color color)
    {
        uint ret = 0;
        ret |= (uint)(Mathf.Floor(SRGBToLinear(color.r) * 255 + 0.0001f)) << 24;
        ret |= (uint)(Mathf.Floor(SRGBToLinear(color.g) * 255 + 0.0001f)) << 16;
        ret |= (uint)(Mathf.Floor(SRGBToLinear(color.b) * 255 + 0.0001f)) << 8;
        ret |= (uint)(Mathf.Floor(color.a * 255 + 0.0001f));
        return ret;
    }

    public static bool RandomBool(float truePossibility)
    {
        return UnityEngine.Random.Range(0.0f, 1.0f) < truePossibility;
    }
}

