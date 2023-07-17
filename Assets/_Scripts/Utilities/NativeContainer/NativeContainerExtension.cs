using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

namespace KaizerWald
{
    public static class NativeContainerExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddIf<TKey, TValue>(this NativeHashMap<TKey, TValue> map, TKey key, TValue value, bool flag)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
        {
            if (!flag || map.TryAdd(key, value)) return;
            map[key] = value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TKey GetKeyMinValue<TKey>(this NativeHashMap<TKey, float> map)
        where TKey : unmanaged, IEquatable<TKey>
        {
            TKey key = default;
            float minValue = math.INFINITY;
            foreach (KVPair<TKey, float> pair in map)
            {
                if (pair.Value > minValue) continue;
                minValue = pair.Value;
                key = pair.Key;
            }
            return key;
        }
    }
}
