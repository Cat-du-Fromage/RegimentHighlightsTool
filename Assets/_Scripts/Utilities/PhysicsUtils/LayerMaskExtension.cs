using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Kaizerwald
{
    public static class LayerMaskExtension
    {
        /// <summary>
        /// Get floorlog2(index) instead of bitshift left
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLayerIndex(this LayerMask layerMask)
        {
            return floorlog2(layerMask.value);
        }
    }
}
