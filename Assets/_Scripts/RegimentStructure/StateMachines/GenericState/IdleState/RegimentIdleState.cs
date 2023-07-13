using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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

namespace KaizerWald
{
    public class RegimentIdleState : IdleState<Regiment>
    {
        public bool AutoFire { get; private set; }
        private int AttackRange;
        private readonly Transform regimentTransform;
        
        private ulong PlayerID => ObjectAttach.OwnerID;
        private int PlayerTeamID => ObjectAttach.TeamID;
        private Formation CurrentFormation => ObjectAttach.CurrentFormation;
        public RegimentIdleState(Regiment regiment) : base(regiment)
        {
            regimentTransform = ObjectAttach.transform;
            AttackRange = regiment.RegimentType.Range;
        }
        
        public void AutoFireOn() => AutoFire = true;
        public void AutoFireOff() => AutoFire = false;

        public override void OnOrderEnter(RegimentOrder order)
        {
            
        }
        
        public override void OnStateEnter()
        {
            
        }

        public override void OnStateUpdate()
        {
            if(!AutoFire) return;
            if(!OnTransitionCheck()) return;
            
            //Sinon Check Si Ennemis à portés
        }

        public override bool OnTransitionCheck()
        {
            //Check whether or not there is a target at range

            NativeList<KeyValuePair<int, float3>> enemyUnitsPositions = GetEnemiesPositions();
            bool hasTarget = CheckEnemiesAtRange(out int regimentTargeted);
            Debug.Log($"hasTarget: {hasTarget} : target is regiment Id: {regimentTargeted}");
            return false;


            bool CheckEnemiesAtRange(out int regimentTargeted)
            {
                regimentTargeted = -1;
                float3 regimentForward = regimentTransform.forward.xOy();
                float3 regimentPosition = regimentTransform.position.xOy();

                //from center of the first Row : direction * midWidth length(Left and Right)
                float3 midWidthDistance = regimentTransform.right.xOy() * CurrentFormation.Width / 2f;
                float3 unit0 = regimentPosition - midWidthDistance; //unit most left
                float3 unitWidth = regimentPosition + midWidthDistance; //unit most Right
                
                //Rotation of the direction the regiment is facing (around Vector3.up) to get both direction of the vision cone
                float3 directionLeft  = (AngleAxis(-Regiment.FovAngleInDegrees, Vector3.up) * regimentForward);
                float3 directionRight = (AngleAxis(Regiment.FovAngleInDegrees, Vector3.up) * regimentForward);

                //Get tip of the cone formed by the intersection made by the 2 previous directions calculated
                float3 intersection = GetIntersection3DFlat(unit0, unitWidth, directionLeft, directionRight);
                float radius = AttackRange + distance(intersection, unit0);// Vector3.Distance(intersection, unit0);
                
                NativeHashMap<int, float> enemyRegimentDistances = new (2, Temp);

                

                foreach ((int unitsRegimentId, Vector3 unitPosition) in enemyUnitsPositions)
                {
                    //Behind Regiment Check
                    bool isBehindRegiment = dot(regimentPosition, unitPosition) < 0;
                    if (isBehindRegiment) continue;
                    //Out Of Range Check
                    float distanceFromEnemy = distancesq(intersection, unitPosition);
                    bool isOutOfRange = distanceFromEnemy > Square(radius);
                    if (isOutOfRange) continue;
                    
                    float2 unitPosition2D = new (unitPosition.x, unitPosition.z);
                    
                    float2x2 leftStartDir = float2x2(unit0.xz, directionLeft.xz);
                    float2x2 rightStartDir = float2x2(unitWidth.xz, directionRight.xz);
                    
                    NativeArray<float2> triangle =
                        GetTrianglePoints(intersection, unit0.xz, directionLeft.xz, unitWidth.xz, directionRight.xz, radius);
                    if (!unitPosition2D.IsPointInTriangle(triangle)) continue; //2) est-ce que l'unité est dans le triangle!
                    
                    bool invalidKey = !enemyRegimentDistances.TryGetValue(unitsRegimentId, out float currentMinDistance);
                    bool updateMinDistance = invalidKey || distanceFromEnemy < currentMinDistance;
                    enemyRegimentDistances.AddIf(unitsRegimentId, distanceFromEnemy, updateMinDistance);
                }

                if (enemyRegimentDistances.IsEmpty) return false;

                regimentTargeted = enemyRegimentDistances.GetKeyMinValue();
                return true;
            }

            NativeArray<float2> GetTrianglePoints(float3 tipPoint, float2 leftStart, float2 leftDir, float2 rightStart, float2 rightDir, float radius)
            {
                NativeArray<float2> points = new(3, Temp, UninitializedMemory);
                points[0] = tipPoint.xz;
                float3 topForwardDirection = normalizesafe((float3)regimentTransform.position.xOy() - tipPoint);
                float3 topForwardFov = tipPoint + topForwardDirection * radius;
                float2 leftCrossDir = topForwardDirection.xz.CrossCounterClockWise();
                points[1] = GetIntersection(topForwardFov.xz, leftStart, leftCrossDir, leftDir);
                float2 rightCrossDir = topForwardDirection.xz.CrossClockWise();
                points[2] = GetIntersection(topForwardFov.xz, rightStart, rightCrossDir, rightDir);
                return points;
            }

            NativeList<KeyValuePair<int, float3>> GetEnemiesPositions()
            {
                int numEnemyUnits = RegimentManager.Instance.GetEnemiesTeamNumUnits(PlayerTeamID);
                NativeParallelMultiHashMap<int, float3> temp = new (numEnemyUnits, Temp);
                foreach ((int teamID, List<Regiment> regiments) in RegimentManager.Instance.RegimentsByTeamID)
                {
                    if (teamID == PlayerTeamID) continue;
                    foreach (Regiment regiment in regiments)
                    {
                        foreach (Unit unit in regiment.Units)
                        {
                            temp.Add(regiment.RegimentID, unit.transform.position);
                        }
                    }
                }
                
                
                NativeList<KeyValuePair<int, float3>> tmp = new(numEnemyUnits, Temp);
                foreach ((int teamID, List<Regiment> regiments) in RegimentManager.Instance.RegimentsByTeamID)
                {
                    if (teamID == PlayerTeamID) continue;
                    foreach (Regiment regiment in regiments)
                    {
                        foreach (Unit unit in regiment.Units)
                        {
                            KeyValuePair<int, float3> keyValuePair = new (regiment.RegimentID, unit.transform.position);
                            tmp.Add(keyValuePair);
                        }
                    }
                }
                return tmp;
            }
        }
        


        public override void OnStateExit()
        {
            
        }
    }
}

/*
     NativeParallelMultiHashMap<int, float3> GetEnemiesPositions2()
    {
        int numEnemyUnits = RegimentManager.Instance.GetEnemiesTeamNumUnits(PlayerTeamID);
        NativeParallelMultiHashMap<int, float3> temp = new(numEnemyUnits, Temp);
        foreach ((int teamID, List<Regiment> regiments) in RegimentManager.Instance.RegimentsByTeamID)
        {
            if (teamID == PlayerTeamID) continue;
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