using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Kaizerwald
{
    public static class Vector2Extension
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Sizzling From Vector2 to Vector 3
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 xay(this Vector2 source, float a)
        {
            return new Vector3(source.x, a, source.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 x0y(this Vector2 source)
        {
            return source.xay(0);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 x1y(this Vector2 source)
        {
            return source.xay(1);
        }
    }
}
