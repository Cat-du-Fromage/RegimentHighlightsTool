using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

namespace Kaizerwald
{
    public static class CSharpContainerExtension
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ ARRAY ◆◆◆◆◆◆                                                    ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEachWithIndex<T>(this T[] array, Action<int> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                action(i);
            }
        }
        
        
        //Exemple Microsoft
        //int[] someArray = new int[5] { 1, 2, 3, 4, 5 };
        //int[] subArray1 = someArray[0..2];               // { 1, 2 }
        //int[] subArray2 = someArray[1..^0];              // { 2, 3, 4, 5 }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Slice<T>(this T[] array, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            return array[startIndex..endIndex];
        }

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
        public static void CopyTo<T>(this T[] arraySource, int sourceStartIndex, T[] arrayDestination, int destinationStartIndex, int length)
        {
            Array.Copy(arraySource, sourceStartIndex, arrayDestination, destinationStartIndex, length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] arraySource, T[] arrayDestination, int length)
        {
            Array.Copy(arraySource, arrayDestination, length);
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(this List<T> list, int left, int right)
        {
            /*
            T temp = list[left];
            list[left] = list[right];
            list[right] = temp;
            */
            (list[left], list[right]) = (list[right], list[left]);
        }
        
        /// <summary>
        /// Exchanges the values of a and b.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T a, ref T b)
        {
            (a, b) = (b, a);
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
