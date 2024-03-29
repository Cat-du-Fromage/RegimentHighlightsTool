using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Kaizerwald.KzwMath;
using static Unity.Mathematics.math;

using static Kaizerwald.StateExtension;
using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using float2 = Unity.Mathematics.float2;

namespace Kaizerwald
{
    public sealed class Regiment_IdleState : RegimentStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public bool AutoFire { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private int AttackRange => RegimentAttach.RegimentType.Range;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Setters ◈◈◈◈◈◈                                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void AutoFireOn() => AutoFire = true;
        public void AutoFireOff() => AutoFire = false;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment_IdleState(RegimentBehaviourTree behaviourTree) : base(behaviourTree, EStates.Idle)
        {
        }

        //Maybe "Stop" button like in Total war?
        public override void OnSetup(Order order) { return; }

        public override void OnEnter() { return; }

        public override void OnUpdate() { return; }

        public override void OnExit() { return; }

        public override EStates ShouldExit()
        {
            //if(RegimentAttach.TeamID != 0) return StateIdentity;
            if(FireExit()) return EStates.Fire;
            
            if (MoveExit()) return EStates.Move;
            
            return StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private bool FireExit()
        {
            //if (!AutoFire) return false;
            bool enemyInRange = CheckEnemiesAtRange(RegimentAttach, AttackRange, out int targetId);
            if (enemyInRange) RegimentBlackboard.SetEnemyTarget(targetId);
            return enemyInRange;
        }
        
        private bool MoveExit()
        {
            bool isChasing = RegimentBlackboard.IsChasing;
            if (RegimentBlackboard.IsChasing)
            {
                RegimentBlackboard.SetChaseDestination(RegimentAttach.CurrentFormation);
                //Debug.Log($"From IDLE Chase Exit: {isChasing} Formation: {RegimentAttach.CurrentFormation}");
            }
            return isChasing;
        }
    }
}
