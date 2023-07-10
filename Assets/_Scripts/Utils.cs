using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Utilities
{
    public static int GetSizeOf<T>() where T : struct
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
    }
}
