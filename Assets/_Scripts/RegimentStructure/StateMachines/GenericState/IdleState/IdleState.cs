using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;


using static KaizerWald.KzwMath;
using static UnityEngine.Quaternion;

using static Unity.Mathematics.math;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using float2 = Unity.Mathematics.float2;

namespace KaizerWald
{
    public class IdleState<T> : State<T>
    where T : MonoBehaviour
    {
        public IdleState(T objectAttach) : base(objectAttach)
        {
        }
        public override void OnAbilityTrigger() { return; }

        public override void OnStateEnter() { return; }
        public override void OnOrderEnter(RegimentOrder order) { return; }
        public override void OnStateUpdate() { return; }
        public override bool OnTransitionCheck() { return false; }
        public override void OnStateExit() { return; }
    }
}