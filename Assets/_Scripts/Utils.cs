using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kaizerwald;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;
using static Kaizerwald.KzwMath;
using static Unity.Mathematics.math;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using float2 = Unity.Mathematics.float2;

public static class Utilities
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSizeOf<T>() 
    where T : struct
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
    }
    
    private static bool CheckEnemyRegimentAtRange(Transform transform, int width, int range, float2 target)
    {
        float3 position = transform.position;
        float3 forward = transform.forward;
        //from center of the first Row : direction * midWidth length(Left and Right)
        float2 midWidthDistance = transform.right.xz() * width / 2f;

        float2 unitLeft = position.xz - midWidthDistance; //unit most left
        float2 unitRight = position.xz + midWidthDistance; //unit most Right
        
        //Rotation of the direction the regiment is facing (around Vector3.up) to get both direction of the vision cone
        float2 directionLeft = mul(AngleAxis(-Regiment.FovAngleInDegrees, up()), forward).xz;
        float2 directionRight = mul(AngleAxis(Regiment.FovAngleInDegrees, up()), forward).xz;

        //wrapper for more readable value passed
        float2x2 leftStartDir = float2x2(unitLeft, directionLeft);
        float2x2 rightStartDir = float2x2(unitRight, directionRight);
        
        //Get tip of the cone formed by the intersection made by the 2 previous directions calculated
        float2 intersection = GetIntersection(leftStartDir, rightStartDir);
        float radius = range + distance(intersection, unitLeft); //unit left choisi arbitrairement(right va aussi)
        
        //Get regiments units and sort their positions taking only the closest one to choose the target
        bool isEnemyAtRange = IsInRange(position.xz, forward.xz, target, intersection, leftStartDir, rightStartDir, radius);
        return isEnemyAtRange;
    }
    
    private static bool IsInRange(float2 regimentPosition, float2 regimentForward, float2 unitPosition, float2 massCenter, in float2x2 leftStartDir, in float2x2 rightStartDir, float radius)
    {
        // 1) Is Inside The Circle (Range)
        float distanceFromEnemy = distancesq(massCenter, unitPosition);
        if (IsOutOfRange(distanceFromEnemy)) return false;
            
        // 2) Behind Regiment Check
        //Regiment.forward: (regPos -> directionForward) , regiment -> enemy: (enemyPos - regPos) 
        float2 regimentToUnitDirection = normalizesafe(unitPosition - regimentPosition);
        if (IsEnemyBehind(regimentToUnitDirection)) return false;
            
        // 3) Is Inside the Triangle of vision (by checking inside both circle and triangle we get the Cone)
        NativeArray<float2> triangle = GetTrianglePoints(regimentPosition, massCenter, leftStartDir, rightStartDir, radius);
        return unitPosition.IsPointInTriangle(triangle);

        bool IsOutOfRange(float distance) => distance > Square(radius);
        bool IsEnemyBehind(in float2 direction) => dot(direction, regimentForward) < 0;
    }

    private static NativeArray<float2> GetTrianglePoints(float2 position, in float2 tipPoint, in float2x2 leftStartDir, in float2x2 rightStartDir, float radius)
    {
        NativeArray<float2> points = new(3, Temp, UninitializedMemory);
        points[0] = tipPoint;
        
        float2 topForwardDirection = normalizesafe(position - tipPoint);
        float2 topForwardFov = tipPoint + topForwardDirection * radius;
        
        float2 leftCrossDir = topForwardDirection.CrossLeft();
        points[1] = GetIntersection(float2x2(topForwardFov, leftCrossDir), leftStartDir);
        
        float2 rightCrossDir = topForwardDirection.CrossRight();
        points[2] = GetIntersection(float2x2(topForwardFov, rightCrossDir), rightStartDir);
        
        return points;
    }
}
