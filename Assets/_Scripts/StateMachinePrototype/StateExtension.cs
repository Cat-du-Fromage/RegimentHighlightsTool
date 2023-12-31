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
    public static class StateExtension
    {
        // CHECK STILL IN RANGE
        
        public static bool IsTargetRegimentInRange(Regiment regimentAttach, Regiment regimentTargeted, int attackRange)
        {
            float2 position = regimentAttach.RegimentTransform.position.xz();
            float3 forward = regimentAttach.RegimentTransform.forward;
            
            //from center of the first Row : direction * midWidth length(Left and Right)
            float2 midWidthDistance = regimentAttach.RegimentTransform.right.xz() * regimentAttach.CurrentFormation.Width / 2f;
            
            float2 unitLeft = position - midWidthDistance; //unit most left
            float2 unitRight = position + midWidthDistance; //unit most Right
            
            //Rotation of the direction the regiment is facing (around Vector3.up) to get both direction of the vision cone
            float2 directionLeft = mul(AngleAxis(-Regiment.FovAngleInDegrees, up()), forward).xz;
            float2 directionRight = mul(AngleAxis(Regiment.FovAngleInDegrees, up()), forward).xz;

            //wrapper for more readable value passed
            float2x2 leftStartDir = float2x2(unitLeft, directionLeft);
            float2x2 rightStartDir = float2x2(unitRight, directionRight);
            
            //Get tip of the cone formed by the intersection made by the 2 previous directions calculated
            float2 intersection = GetIntersection(leftStartDir, rightStartDir);
            float radius = attackRange + distance(intersection, unitLeft); //unit left choisi arbitrairement(right va aussi)
            
            return IsEnemyInRange(regimentAttach, regimentTargeted, intersection, leftStartDir, rightStartDir, radius);
        }
        
        private static bool IsEnemyInRange(Regiment regimentAttach, Regiment regimentTargeted, float2 triangleTip, in float2x2 leftStartDir, in float2x2 rightStartDir, float radius)
        {
            float radiusSq = Square(radius);
            float2 regimentPosition = regimentAttach.RegimentPosition.xz;
            float2 forward = regimentAttach.RegimentTransform.forward.xz();
            NativeArray<float3> enemyUnitsPositions = GetTargetUnitsPosition(regimentTargeted);
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
                NativeArray<float2> triangle = GetTrianglePoints(regimentPosition, triangleTip, leftStartDir, rightStartDir, radius);
                if (!unitPosition2D.IsPointInTriangle(triangle)) continue;
                return true;
            }
            return false;

            // --------------------------------------------------------------
            // INTERNAL METHODS
            // --------------------------------------------------------------
            bool IsOutOfRange(float distance) => distance > radiusSq;
            bool IsEnemyBehind(in float2 direction) => dot(direction, forward) < 0;
        }
        
        private static NativeArray<float3> GetTargetUnitsPosition(Regiment regimentTargeted)
        {
            NativeArray<float3> targetUnitsPosition = new(regimentTargeted.Units.Count, Temp, UninitializedMemory);
            for (int i = 0; i < regimentTargeted.Units.Count; i++)
            {
                targetUnitsPosition[i] = regimentTargeted.UnitsTransform[i].position;
            }
            return targetUnitsPosition;
        }
        
        // GET CLOSEST ENEMY
        
        public static bool CheckEnemiesAtRange(Regiment regimentAttach, int attackRange, out int regimentTargeted)
        {
            float2 position = regimentAttach.RegimentTransform.position.xz();
            float3 forward = regimentAttach.RegimentTransform.forward;
            
            regimentTargeted = -1;
            //from center of the first Row : direction * midWidth length(Left and Right)
            float2 midWidthDistance = regimentAttach.RegimentTransform.right.xz() * regimentAttach.CurrentFormation.Width / 2f;
            
            float2 unitLeft = position - midWidthDistance; //unit most left
            float2 unitRight = position + midWidthDistance; //unit most Right
            
            //Rotation of the direction the regiment is facing (around Vector3.up) to get both direction of the vision cone
            float2 directionLeft = mul(AngleAxis(-Regiment.FovAngleInDegrees, up()), forward).xz;
            float2 directionRight = mul(AngleAxis(Regiment.FovAngleInDegrees, up()), forward).xz;

            //wrapper for more readable value passed
            float2x2 leftStartDir = float2x2(unitLeft, directionLeft);
            float2x2 rightStartDir = float2x2(unitRight, directionRight);
            
            //Get tip of the cone formed by the intersection made by the 2 previous directions calculated
            float2 intersection = GetIntersection(leftStartDir, rightStartDir);
            float radius = attackRange + distance(intersection, unitLeft); //unit left choisi arbitrairement(right va aussi)
            
            //Get regiments units and sort their positions taking only the closest one to choose the target
            NativeHashMap<int, float> enemyRegimentDistances = 
                GetEnemiesDistancesSorted(regimentAttach, intersection, leftStartDir, rightStartDir, radius);
            
            if (enemyRegimentDistances.IsEmpty) return false;
            regimentTargeted = enemyRegimentDistances.GetKeyMinValue();
            return true;
        }
        
        public static NativeHashMap<int, float> GetEnemiesDistancesSorted(Regiment regimentAttach, float2 triangleTip, 
            in float2x2 leftStartDir, in float2x2 rightStartDir, float radius)
        {
            float radiusSq = Square(radius);
            using NativeParallelMultiHashMap<int, float3> enemyUnitsPositions = GetEnemiesPositions(regimentAttach);
            
            NativeHashMap<int, float> enemyRegimentDistances = new (8, Temp);
            foreach (KeyValue<int, float3> unitRegIdPosition in enemyUnitsPositions)
            {
                float2 regimentPosition = regimentAttach.RegimentTransform.position.xz();
                float2 unitPosition2D = unitRegIdPosition.Value.xz;
                // 1) Is Inside The Circle (Range)
                float distanceFromEnemy = distancesq(triangleTip, unitPosition2D);
                if (IsOutOfRange(distanceFromEnemy)) continue;
                
                // 2) Behind Regiment Check
                //Regiment.forward: (regPos -> directionForward) , regiment -> enemy: (enemyPos - regPos) 
                float2 regimentToUnitDirection = normalizesafe(unitPosition2D - regimentPosition);
                if (IsEnemyBehind(regimentToUnitDirection)) continue;
                
                // 3) Is Inside the Triangle of vision (by checking inside both circle and triangle we get the Cone)
                NativeArray<float2> triangle = GetTrianglePoints(regimentPosition, triangleTip, leftStartDir, rightStartDir, radius);
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
            
            bool IsEnemyBehind(in float2 direction) => dot(direction, regimentAttach.RegimentTransform.forward.xz()) < 0;
            
            bool IsMinDistanceUpdated(int key, float distance)
            {
                bool invalidKey = !enemyRegimentDistances.TryGetValue(key, out float currentMinDistance);
                return invalidKey || distance < currentMinDistance;
            }
        }

        private static NativeArray<float2> GetTrianglePoints(in float2 position2D, in float2 tipPoint, in float2x2 leftStartDir, in float2x2 rightStartDir, float radius)
        {
            NativeArray<float2> points = new(3, Temp, UninitializedMemory);
            points[0] = tipPoint;
            
            float2 topForwardDirection = normalizesafe(position2D - tipPoint);
            float2 topForwardFov = tipPoint + topForwardDirection * radius;
            
            float2 leftCrossDir = topForwardDirection.CrossLeft();
            points[1] = GetIntersection(float2x2(topForwardFov, leftCrossDir), leftStartDir);
            
            float2 rightCrossDir = topForwardDirection.CrossRight();
            points[2] = GetIntersection(float2x2(topForwardFov, rightCrossDir), rightStartDir);
            
            return points;
        }
        
        private static NativeParallelMultiHashMap<int, float3> GetEnemiesPositions(Regiment regimentAttach)
        {
            int numEnemyUnits = HighlightRegimentManager.Instance.GetEnemiesTeamNumUnits(regimentAttach.TeamID);
            NativeParallelMultiHashMap<int, float3> temp = new(numEnemyUnits, Temp);
            foreach ((int teamID, List<Regiment> regiments) in HighlightRegimentManager.Instance.RegimentsByTeamID)
            {
                if (teamID == regimentAttach.TeamID) continue;
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
