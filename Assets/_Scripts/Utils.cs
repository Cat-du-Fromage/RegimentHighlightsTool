using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class Utilities
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSizeOf<T>() 
    where T : struct
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
    }
}
