using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace Kaizerwald
{
    public static class CSharpContainerUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Comparer<KeyValuePair<TKey, TValue>> GetKeyValuePairComparer<TKey, TValue>()
        where TValue : IComparable<TValue>
        {
            return Comparer<KeyValuePair<TKey, TValue>>.Create((a, b) => 
                a.Value.CompareTo(b.Value));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T leftElement, ref T rightElement)
        {
            (leftElement, rightElement) = (rightElement, leftElement);
        }
        
        
    }
}
