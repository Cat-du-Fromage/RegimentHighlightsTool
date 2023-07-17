using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KaizerWald
{
    public static class Vector3Extension
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Sizzling Vector3
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 xOz(this Vector3 coordToFlat)
        {
            return new Vector3(coordToFlat.x, 0, coordToFlat.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 x1z(this Vector3 coordToFlat)
        {
            return new Vector3(coordToFlat.x, 1f, coordToFlat.z);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        /// <summary>
        /// Sizzling From Vector3 To Vector2
        /// </summary>
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 xy(this Vector3 source)
        {
            return new Vector2(source.x, source.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 xz(this Vector3 source)
        {
            return new Vector2(source.x, source.z);
        }
        
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//
//  VECTOR3 BEHAVIOUR
//
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Set Axis to a given destination using XZ axis only
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 FlatMove(this Vector3 coordToMove, Vector3 newPosition)
        {
            return new Vector3(newPosition.x, coordToMove.y, newPosition.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetX(this Vector3 target, float value)
        {
            return new Vector3(value,target.y,target.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetY(this Vector3 target, float value)
        {
            return new Vector3(target.x,value,target.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetZ(this Vector3 target, float value)
        {
            return new Vector3(target.x,target.y,value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 DirectionTo(this Vector3 source, Vector3 destination)
        {
            return Vector3.Normalize(destination - source);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 DirectionTo(this float3 source, float3 destination)
        {
            return normalize(destination - source);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceTo(this Vector3 source, Vector3 destination)
        {
            return Vector3.Magnitude(destination - source);
        }

    }
}
