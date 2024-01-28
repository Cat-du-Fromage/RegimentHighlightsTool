using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Kaizerwald
{
    public static class Float2Extension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 DirectionTo(this float2 source, float2 destination)
        {
            return normalize(destination - source);
        }
    }
}
