using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWald.KzwMath;
using static Unity.Mathematics.math;
using static Unity.Mathematics.int2;

using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

namespace KaizerWald
{
    public sealed class RegimentFireState : FireState<Regiment>
    {
        private FormationData CacheEnemyFormation;
        private Formation CurrentEnemyFormation => EnemyTarget.CurrentFormation;
        private RegimentStateMachine LinkedRegimentStateMachine => ObjectAttach.StateMachine;
        private List<UnitStateMachine> UnitsStateMachines => LinkedRegimentStateMachine.UnitsStateMachine;
        
        public RegimentFireState(Regiment objectAttach) : base(objectAttach)
        {
            
        }
        
        public override void OnStateEnter(Order<Regiment> order)
        {
            AttackRegimentOrder attackOrder = (AttackRegimentOrder)order;
            EnemyTarget = attackOrder.EnemyTarget;
            CacheEnemyFormation = attackOrder.EnemyTarget.CurrentFormation;
            OrderUnitStateTransition();
        }

        public override void OnStateUpdate()
        {
            //Check EnemyFormation: changes detected? how ?
            bool3 hasEnemyFormationChange = new bool3
            (
                CacheEnemyFormation.NumUnitsAlive != CurrentEnemyFormation.NumUnitsAlive,
                CacheEnemyFormation.Width != CurrentEnemyFormation.Width,
                CacheEnemyFormation.Depth != CurrentEnemyFormation.Depth
            );

            if (any(hasEnemyFormationChange))
            {
                UpdateUnitsTarget();
                CacheEnemyFormation = CurrentEnemyFormation;
                Debug.Log("FireState : Change Detected = true");
            }
            
            if (!OnTransitionCheck()) return;
            LinkedStateMachine.TransitionDefaultState();
        }

        public override bool OnTransitionCheck()
        {
            // Is Regiment Still Alive
            return EnemyTarget == null || EnemyTarget.Units.Count == 0;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ EXTERNAL CALL ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnUnitRequest(UnitStateMachine unitStateMachine)
        {
            AttackUnitOrder order = GetTargetForUnit(unitStateMachine);
            unitStateMachine.CurrentState.OnStateEnter(order);
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ INTERNAL CALL ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ State Transition Methods ◇◇◇◇◇◇                                                                    │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void OrderUnitStateTransition()
        {
            foreach (UnitStateMachine unitStateMachine in UnitsStateMachines)
            {
                AttackUnitOrder order = GetTargetForUnit(unitStateMachine);
                unitStateMachine.TransitionState(order);
            }
        }

        private void UpdateUnitsTarget()
        {
            foreach (UnitStateMachine unitStateMachine in UnitsStateMachines)
            {
                AttackUnitOrder order = GetTargetForUnit(unitStateMachine);
                unitStateMachine.CurrentState.OnStateEnter(order);
            }
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Get Target Methods ◇◇◇◇◇◇                                                                          │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private AttackUnitOrder GetTargetForUnit(UnitStateMachine caller)
        {
            Unit target = GetTarget(caller.transform.position.xz());
            return new AttackUnitOrder(caller.ObjectAttach, target);
        }

        private Unit GetTarget(float2 unitPosition)
        {
            NativeHashMap<int, float2> indexPairPosition = GetTargetRegimentUnitsHull();
            int index = -1;
            float minDistance = INFINITY;
            foreach (KVPair<int, float2> pair in indexPairPosition)
            {
                float distance = distancesq(unitPosition, pair.Value);
                if (distance > minDistance) continue;
                minDistance = distance;
                index = pair.Key;
            }
            return index == -1 ? null : EnemyTarget.Units[index];
        }
        
        //ISSUE : REORGANISATION NOT DONE So we try to access index that no longer exist
        private NativeHashMap<int, float2> GetTargetRegimentUnitsHull()
        {
            using NativeHashSet<int> hullIndices = GetUnitsHullIndices();
            //Debug.Log($"base NumUnits: {CurrentEnemyFormation.BaseNumUnits}; Alive: {CurrentEnemyFormation.NumUnitsAlive}");
            NativeHashMap<int, float2> result = new (hullIndices.Count, Temp);
            foreach (int unitIndex in hullIndices)
            {
                //Debug.Log($"Trying Access Index: {unitIndex}; Max: {CurrentEnemyFormation.NumUnitsAlive}");
                float2 unitPosition = EnemyTarget.UnitsTransform[unitIndex].position.xz();
                result.Add(unitIndex, unitPosition);
            }
            return result;
            
            // INTERNAL METHODS
            NativeHashSet<int> GetUnitsHullIndices()
            {
                int numUnit = CurrentEnemyFormation.NumUnitsAlive;
                int2 widthDepth = CurrentEnemyFormation.WidthDepth;
                int lastWidthIndex = widthDepth[0] - 1;
                int lastDepthIndex = widthDepth[1] - 1;
                int completeDepth = CurrentEnemyFormation.NumCompleteLine;
            
                NativeHashSet<int> indices = new (completeDepth * widthDepth[0], Temp);
                for (int i = 0; i < numUnit; i++)
                {
                    int2 coord = GetXY2(i, widthDepth[0]);
                    bool xyZero = any(coord == zero);
                    bool xMaxWidth = coord.x == lastWidthIndex;
                    bool yMaxDepth = coord.y == lastDepthIndex;
                    if (!any(new bool3(xyZero, xMaxWidth, yMaxDepth))) continue; 
                    indices.Add(i);
                }
                return indices;
            }
        }
    }
}
