using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

namespace KaizerWald
{
    public static class CSharpContainerExtension
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ ARRAY ◆◆◆◆◆◆                                                    ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEachWithIndex<T>(this T[] array, Func<int, T> action)
        {
            for (int i = 0; i < array.Length; i++)action(i);
        }*/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            Array.ForEach(array, action);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEachSafe<T>(this T[] array, Action<T> action)
        {
            if (array == null) return;
            Array.ForEach(array, action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reverse<T>(this T[] array)
        {
            Array.Reverse(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Resize<T>(this T[] array, int newSize)
        {
            Array.Resize(ref array, newSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this T[] array)
        {
            Array.Clear(array, 0, array.Length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(this T[] array, T value)
        {
            Array.Fill(array, value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Concat<T>(this T[] x, T[] y)
        {
            int oldLen = x.Length;
            Array.Resize(ref x, x.Length + y.Length);
            Array.Copy(y, 0, x, oldLen, y.Length);
            return x;
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ LIST ◆◆◆◆◆◆                                                    ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        /// <summary>
        /// Add Element if not already present on the list return if the element as been added
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AddUnique<T>(this List<T> list, T obj)
        {
            if (list.Contains(obj)) return false;
            list.Add(obj);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AddIf<T>(this List<T> list, T obj, bool flag)
        {
            if (!flag) return false;
            list.Add(obj);
            return true;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                           ◆◆◆◆◆◆ DICTIONARY ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddSafe<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            if (dictionary.TryGetValue(key, out List<TValue> listOutput))
            {
                listOutput.Add(value);
            }
            else
            {
                dictionary.Add(key, new List<TValue>(2));
                dictionary[key].Add(value);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AddIf<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value, bool flag)
        {
            if (!flag) return false;
            dictionary[key] = value;
            return true;
        }
    }
}
