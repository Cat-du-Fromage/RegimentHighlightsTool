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
    public class FireRegimentState : RegimentState
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment RegimentTargeted { get; private set; }
        public FormationData CacheEnemyFormation { get; private set; }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private int AttackRange => RegimentAttach.RegimentType.Range;
        private Formation CurrentEnemyFormation => RegimentTargeted.CurrentFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public FireRegimentState(RegimentStateMachine regimentStateMachine) : base(regimentStateMachine, EStates.Fire)
        {
            
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override bool ConditionStateEnter()
        {
            bool isAtRange = CheckEnemyRegimentAtRange();
            return isAtRange;
        }

        public override void SetupState(Order order)
        {
            AttackOrder attackOrder = (AttackOrder)order;
            RegimentTargeted = attackOrder.TargetEnemyRegiment;
            CacheEnemyFormation = attackOrder.TargetEnemyRegiment.CurrentFormation;
        }
        
        public override void EnterState()
        {
            
        }

        public override void UpdateState()
        {
            //Check EnemyFormation: changes detected? how ?
            if (HasEnemyFormationChange() && CurrentEnemyFormation.NumUnitsAlive != 0)
            {
                CacheEnemyFormation = CurrentEnemyFormation;
            }
            CheckSwitchState();
        }
        
        //Possible Next State: Move(if target order); Idle(if (target dead or Out of range) and no target Order)
        public override bool CheckSwitchState()
        {
            
            return SwitchToIdleState(); // Is Regiment Still Alive
        }

        public override void ExitState()
        {
            
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private bool HasEnemyFormationChange()
        {
            bool3 hasEnemyFormationChange = new bool3
            (
                CacheEnemyFormation.NumUnitsAlive != CurrentEnemyFormation.NumUnitsAlive,
                CacheEnemyFormation.WidthDepth != CurrentEnemyFormation.WidthDepth
            );
            return any(hasEnemyFormationChange);
        }

        private bool SwitchToIdleState()
        {
            bool isTargetDead = RegimentTargeted == null || RegimentTargeted.Units.Count == 0;
            if (isTargetDead) LinkedRegimentStateMachine.ToDefaultState();
            return isTargetDead;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Check Enemies at Range ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private bool CheckEnemyRegimentAtRange()
        {
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
            bool isEnemyAtRange = GetEnemiesDistancesSorted(Position.xz, intersection, leftStartDir, rightStartDir, radius);
            return isEnemyAtRange;
        }
        
        private bool GetEnemiesDistancesSorted(float2 regimentPosition, float2 triangleTip, in float2x2 leftStartDir, in float2x2 rightStartDir, float radius)
        {
            float radiusSq = Square(radius);
            NativeArray<float3> enemyUnitsPositions = GetTargetUnitsPosition();
            
            NativeHashMap<int, float> enemyRegimentDistances = new (8, Temp);
            foreach (float3 unitPosition in enemyUnitsPositions)
            {
                float2 unitPosition2D = unitPosition.xz;
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
                return true;
            }
            return false;

            // --------------------------------------------------------------
            // INTERNAL METHODS
            // --------------------------------------------------------------
            bool IsOutOfRange(float distance) => distance > radiusSq;
            bool IsEnemyBehind(in float2 direction) => dot(direction, Forward.xz) < 0;
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
        
        private NativeArray<float3> GetTargetUnitsPosition()
        {
            NativeArray<float3> targetUnitsPosition = new(RegimentTargeted.Units.Count, Temp, UninitializedMemory);
            for (int i = 0; i < RegimentTargeted.Units.Count; i++)
            {
                targetUnitsPosition[i] = RegimentTargeted.UnitsTransform[i].position;
            }
            return targetUnitsPosition;
        }
    }
}
