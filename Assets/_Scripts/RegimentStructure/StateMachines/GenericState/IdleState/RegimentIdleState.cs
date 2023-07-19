using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWald.KzwMath;

using static UnityEngine.Vector3;
using static UnityEngine.Quaternion;

using static Unity.Mathematics.math;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using float2 = Unity.Mathematics.float2;

namespace KaizerWald
{
    public class RegimentIdleState : IdleState<Regiment>
    {
        public bool AutoFire { get; private set; }
        private int AttackRange;
        private Formation CurrentFormation => ObjectAttach.CurrentFormation;
        
        public RegimentIdleState(Regiment regiment) : base(regiment)
        {
            AttackRange = regiment.RegimentType.Range;
        }
        
        public override void OnStateUpdate()
        {
            if(!AutoFire) return;
            if(!OnTransitionCheck()) return;
            
            //Sinon Check Si Ennemis à portés
        }

        public override bool OnTransitionCheck()
        {
            bool hasTarget = CheckEnemiesAtRange(out int targetId);
            bool regimentExist = RegimentManager.Instance.RegimentsByID.TryGetValue(targetId, out Regiment target);
            if (!hasTarget || !regimentExist) return false;
            
            RegimentAttackOrder order = new RegimentAttackOrder(target);
            LinkedStateMachine.TransitionState(order);
            
            Debug.Log($"{ObjectAttach.name} hasTarget: {hasTarget} : target is regiment Id: {targetId}");
            return true;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public void AutoFireOn() => AutoFire = true;
        public void AutoFireOff() => AutoFire = false;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Check Enemies at Range ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        //TODO: (TESTER AVANT)Changer la façOn de délimiter la Portée
        //TODO: Total war semble ne pas avoir de notion d'angle de vue, mais celle-ci semble être calculé selon la porté et la taille des lignes
        //TODO: 1) Utilise le Leader comme point de référence et faire "Range"/2 * regiment.back() pour trouver le centre du cercle
        //TODO: 2) Calculer les angles gauche/droite depuis le centre (ex:45°) ou utiliser les Unités
        private bool CheckEnemiesAtRange(out int regimentTargeted)
        {
            regimentTargeted = -1;
            float3 regimentForward = ObjectTransform.forward;
            float2 regimentPosition = ObjectTransform.position.xz();

            //from center of the first Row : direction * midWidth length(Left and Right)
            float2 midWidthDistance = ObjectTransform.right.xz() * CurrentFormation.Width / 2f;
            float2 unit0 = regimentPosition - midWidthDistance; //unit most left
            float2 unitWidth = regimentPosition + midWidthDistance; //unit most Right
            
            //Rotation of the direction the regiment is facing (around Vector3.up) to get both direction of the vision cone
            float2 directionLeft  = (AngleAxis(-Regiment.FovAngleInDegrees, Vector3.up) * regimentForward).xz();
            float2 directionRight = (AngleAxis(Regiment.FovAngleInDegrees, Vector3.up) * regimentForward).xz();
            
            //wrapper for more readable value passed
            float2x2 leftStartDir = float2x2(unit0, directionLeft);
            float2x2 rightStartDir = float2x2(unitWidth, directionRight);
            
            //Get tip of the cone formed by the intersection made by the 2 previous directions calculated
            float2 intersection = GetIntersection(leftStartDir, rightStartDir);
            float radius = AttackRange + distance(intersection, unit0);
            
            //Get regiments units and sort their positions taking only the closest one to choose the target
            NativeHashMap<int, float> enemyRegimentDistances = 
                GetEnemiesDistancesSorted(regimentPosition, intersection, leftStartDir, rightStartDir, radius);
            
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
                bool isOutOfRange = distanceFromEnemy > radiusSq;
                if (isOutOfRange) continue;
                
                // 2) Behind Regiment Check
                //Regiment.forward: (regPos -> directionForward) , regiment -> enemy: (enemyPos - regPos) 
                float2 regimentToUnitDirection = normalizesafe(unitPosition2D - regimentPosition);
                bool isEnemyBehind = dot(regimentToUnitDirection, ObjectTransform.forward.xz()) < 0;
                if (isEnemyBehind) continue;
                
                // 3) Is Inside the Triangle of vision (by checking inside both circle and triangle we get the Cone)
                NativeArray<float2> triangle = GetTrianglePoints(triangleTip, leftStartDir, rightStartDir, radius);
                if (!unitPosition2D.IsPointInTriangle(triangle)) continue;
                //Check: Update Distance
                bool invalidKey = !enemyRegimentDistances.TryGetValue(unitRegIdPosition.Key, out float currentMinDistance);
                bool updateMinDistance = invalidKey || distanceFromEnemy < currentMinDistance;
                
                enemyRegimentDistances.AddIf(unitRegIdPosition.Key, distanceFromEnemy, updateMinDistance);
            }
            return enemyRegimentDistances;
        }

        private NativeArray<float2> GetTrianglePoints(in float2 tipPoint, in float2x2 leftStartDir, in float2x2 rightStartDir, float radius)
        {
            NativeArray<float2> points = new(3, Temp, UninitializedMemory);
            points[0] = tipPoint;
            
            float2 topForwardDirection = normalizesafe((float2)ObjectTransform.position.xz() - tipPoint);
            float2 topForwardFov = tipPoint + topForwardDirection * radius;
            
            float2 leftCrossDir = topForwardDirection.CrossCounterClockWise();
            points[1] = GetIntersection(float2x2(topForwardFov, leftCrossDir), leftStartDir);
            
            float2 rightCrossDir = topForwardDirection.CrossClockWise();
            points[2] = GetIntersection(float2x2(topForwardFov, rightCrossDir), rightStartDir);
            
            return points;
        }
        
        private NativeParallelMultiHashMap<int, float3> GetEnemiesPositions()
        {
            int numEnemyUnits = RegimentManager.Instance.GetEnemiesTeamNumUnits(ObjectAttach.TeamID);
            NativeParallelMultiHashMap<int, float3> temp = new(numEnemyUnits, Temp);
            foreach ((int teamID, List<Regiment> regiments) in RegimentManager.Instance.RegimentsByTeamID)
            {
                if (teamID == ObjectAttach.TeamID) continue;
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

/*
    bool CheckEnemiesAtRange2(out int regimentTargeted)
    {
        regimentTargeted = -1;
        float3 regimentForward = regimentTransform.forward.Flat();
        float3 regimentPosition = regimentTransform.position.Flat();
        
        float3x2 leftRightUnit = GetMostLeftRightUnit(regimentPosition);
        float3x2 directions = GetLeftRightDirections(regimentForward);

        //Get tip of the cone formed by the intersection made by the 2 previous directions calculated
        float3 intersection = GetIntersection3DFlat(leftRightUnit.c0, leftRightUnit.c1, directions.c0, directions.c1);
        float radius = AttackRange + distance(intersection, leftRightUnit.c0);
        
        NativeHashMap<int, float> enemyRegimentDistances = new (2, Temp);

        //Vector3 Base regimentPosition + regimentForward * radius
        
        foreach ((int unitsRegimentId, Vector3 unitPosition) in enemyUnitsPositions)
        {
            float distanceFromEnemy = distance(intersection, unitPosition);
            bool isWithinRange = distanceFromEnemy > radius;
            if (!isWithinRange) continue;
            bool isInFrontOfRegiment = dot(regimentPosition, unitPosition) > 0;
            if (!isInFrontOfRegiment) continue;
            
            //2) est-ce que l'unité est dans le triangle!
            float2 unitPosition2D = new (unitPosition.x, unitPosition.z);
            //FAUX
            if (!unitPosition2D.IsPointInTriangle(intersection.xz,leftRightUnit.c0.xz,leftRightUnit.c1.xz)) continue;
            
            bool invalidKey = !enemyRegimentDistances.TryGetValue(unitsRegimentId, out float currentMinDistance);
            bool updateMinDistance = invalidKey || distanceFromEnemy < currentMinDistance;
            enemyRegimentDistances.AddIf(unitsRegimentId, distanceFromEnemy, updateMinDistance);
        }

        if (enemyRegimentDistances.IsEmpty) return false;

        regimentTargeted = enemyRegimentDistances.GetKeyMinValue();
        return true;
        
        float3x2 GetMostLeftRightUnit(float3 regimentPosition)
        {
            //from center of the first Row : direction * midWidth length(Left and Right)
            float3 midWidthDistance = regimentTransform.right.Flat() * CurrentFormation.Width / 2f;
            return new (regimentPosition - midWidthDistance, regimentPosition + midWidthDistance);
        }
        
        float3x2 GetLeftRightDirections(Vector3 regimentForward)
        {
            float3 directionLeft  = (AngleAxis(-Regiment.FovAngleInDegrees, Vector3.up) * regimentForward);
            float3 directionRight = (AngleAxis(Regiment.FovAngleInDegrees, Vector3.up) * regimentForward);
            return new float3x2(directionLeft, directionRight);
        }
    }
*/