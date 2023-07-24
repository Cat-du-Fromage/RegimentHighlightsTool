using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWald.KzwMath;
using static Unity.Mathematics.math;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using float2 = Unity.Mathematics.float2;

namespace KaizerWald
{
    public class IdleRegimentState : RegimentState
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public bool AutoFire { get; private set; }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private int AttackRange => RegimentAttach.RegimentType.Range;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public IdleRegimentState(RegimentStateMachine regimentStateMachine) : base(regimentStateMachine, EStates.Idle)
        {
            
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override void UpdateState()
        {
            CheckSwitchState();
        }

        public override bool CheckSwitchState()
        {
            //NE PAS CHANGER EN TERNAIRE : sera un else if quand melee sera introduit 
            if (AutoFire) 
            {
                return SwitchToFireState();
            }
            else
            {
                return false;
            }
        }

        private bool SwitchToFireState()
        {
            if (!CheckEnemiesAtRange(out int targetId)) return false;
            if(!RegimentManager.Instance.RegimentsByID.TryGetValue(targetId, out Regiment target)) return false;

            AttackOrder order = new AttackOrder(target);
            LinkedRegimentStateMachine.RequestChangeState(order);
            //Debug.Log($"{RegimentAttach.name} hasTarget: {hasTarget} : target is regiment Id: {targetId}");
            return true;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void AutoFireOn() => AutoFire = true;
        public void AutoFireOff() => AutoFire = false;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Check Enemies at Range ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private bool CheckEnemiesAtRange(out int regimentTargeted)
        {
            regimentTargeted = -1;
            //from center of the first Row : direction * midWidth length(Left and Right)
            float2 midWidthDistance = Right.xz * RegimentAttach.CurrentFormation.Width / 2f;
            
            float2 unitLeft = Position.xz - midWidthDistance; //unit most left
            float2 unitRight = Position.xz + midWidthDistance; //unit most Right
            
            //Rotation of the direction the regiment is facing (around Vector3.up) to get both direction of the vision cone
            float2 directionLeft = mul(AngleAxis(-Regiment.FovAngleInDegrees, up()), Forward).xz;
            float2 directionRight = mul(AngleAxis(Regiment.FovAngleInDegrees, up()), Forward).xz;

            //wrapper for more readable value passed
            float2x2 leftStartDir = float2x2(unitLeft, directionLeft);
            float2x2 rightStartDir = float2x2(unitRight, directionRight);
            
            //Get tip of the cone formed by the intersection made by the 2 previous directions calculated
            float2 intersection = GetIntersection(leftStartDir, rightStartDir);
            float radius = AttackRange + distance(intersection, unitLeft); //unit left choisi arbitrairement(right va aussi)
            
            //Get regiments units and sort their positions taking only the closest one to choose the target
            NativeHashMap<int, float> enemyRegimentDistances = 
                GetEnemiesDistancesSorted(Position.xz, intersection, leftStartDir, rightStartDir, radius);
            
            if (enemyRegimentDistances.IsEmpty) return false;
            regimentTargeted = enemyRegimentDistances.GetKeyMinValue();
            return true;
        }
        
        private NativeHashMap<int, float> GetEnemiesDistancesSorted(float2 regimentPosition, float2 triangleTip, 
            in float2x2 leftStartDir, in float2x2 rightStartDir, float radius)
        {
            float radiusSq = Square(radius);
            using NativeParallelMultiHashMap<int, float3> enemyUnitsPositions = GetEnemiesPositions();
            
            NativeHashMap<int, float> enemyRegimentDistances = new (8, Temp);
            foreach (KeyValue<int, float3> unitRegIdPosition in enemyUnitsPositions)
            {
                float2 unitPosition2D = unitRegIdPosition.Value.xz;
                // 1) Is Inside The Circle (Range)
                float distanceFromEnemy = distancesq(triangleTip, unitPosition2D);
                if (IsOutOfRange(distanceFromEnemy)) continue;
                
                // 2) Behind Regiment Check
                //Regiment.forward: (regPos -> directionForward) , regiment -> enemy: (enemyPos - regPos) 
                float2 regimentToUnitDirection = normalizesafe(unitPosition2D - regimentPosition);
                if (IsEnemyBehind(regimentToUnitDirection)) continue;
                
                // 3) Is Inside the Triangle of vision (by checking inside both circle and triangle we get the Cone)
                NativeArray<float2> triangle = GetTrianglePoints(triangleTip, leftStartDir, rightStartDir, radius);
                if (!unitPosition2D.IsPointInTriangle(triangle)) continue;
                //Check: Update Distance
                bool updateMinDistance = IsMinDistanceUpdated(unitRegIdPosition.Key, distanceFromEnemy);
                enemyRegimentDistances.AddIf(unitRegIdPosition.Key, distanceFromEnemy, updateMinDistance);
            }
            return enemyRegimentDistances;

            // --------------------------------------------------------------
            // INTERNAL METHODS
            // --------------------------------------------------------------
            bool IsOutOfRange(float distance) => distance > radiusSq;
            
            bool IsEnemyBehind(in float2 direction) => dot(direction, RegimentTransform.forward.xz()) < 0;
            
            bool IsMinDistanceUpdated(int key, float distance)
            {
                bool invalidKey = !enemyRegimentDistances.TryGetValue(key, out float currentMinDistance);
                return invalidKey || distance < currentMinDistance;
            }
        }

        private NativeArray<float2> GetTrianglePoints(in float2 tipPoint, in float2x2 leftStartDir, in float2x2 rightStartDir, float radius)
        {
            NativeArray<float2> points = new(3, Temp, UninitializedMemory);
            points[0] = tipPoint;
            
            float2 topForwardDirection = normalizesafe(Position.xz - tipPoint);
            float2 topForwardFov = tipPoint + topForwardDirection * radius;
            
            float2 leftCrossDir = topForwardDirection.CrossLeft();
            points[1] = GetIntersection(float2x2(topForwardFov, leftCrossDir), leftStartDir);
            
            float2 rightCrossDir = topForwardDirection.CrossRight();
            points[2] = GetIntersection(float2x2(topForwardFov, rightCrossDir), rightStartDir);
            
            return points;
        }
        
        private NativeParallelMultiHashMap<int, float3> GetEnemiesPositions()
        {
            int numEnemyUnits = RegimentManager.Instance.GetEnemiesTeamNumUnits(RegimentAttach.TeamID);
            NativeParallelMultiHashMap<int, float3> temp = new(numEnemyUnits, Temp);
            foreach ((int teamID, List<Regiment> regiments) in RegimentManager.Instance.RegimentsByTeamID)
            {
                if (teamID == RegimentAttach.TeamID) continue;
                foreach (Regiment regiment in regiments)
                {
                    foreach (Transform unit in regiment.UnitsTransform)
                    {
                        temp.Add(regiment.RegimentID, unit.position);
                    }
                }
            }
            return temp;
        }
    }
}
