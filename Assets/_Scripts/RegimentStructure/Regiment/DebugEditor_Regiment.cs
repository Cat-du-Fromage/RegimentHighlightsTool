#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KaizerWald.KzwMath;
using static UnityEngine.Vector3;
using static UnityEngine.Quaternion;

namespace KaizerWald
{
    public partial class Regiment : MonoBehaviour
    {
        public bool FieldOfViewDebug = false;
        public bool FiringStateTest = false;
        
        private void OnDrawGizmos()
        {
            Debug_FiringStateTargetDetection();
            if (!FieldOfViewDebug) return;
            float3 regimentPosition = transform.position.x1y();
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(regimentPosition, 0.2f);
            
            float3 regimentForward = transform.forward.xOy();
            
            float3 midWidthDistance = transform.right.xOy() * (CurrentFormation.Width / 2f);
            float3 unit0 = regimentPosition - midWidthDistance;
            float3 unitWidth = regimentPosition + midWidthDistance;

            DrawStartingFOV(unit0, unitWidth);
            DrawFieldOfView(regimentPosition.xz, regimentForward, unit0.xz, unitWidth.xz);
        }

        private void DrawFieldOfView(float2 regimentPosition, float3 regimentForward, float2 unit0, float2 unitWidth)
        {
            Gizmos.color = Color.white;

            float3 directionLeft3D = AngleAxis(-FovAngleInDegrees, Vector3.up) * regimentForward;
            float3 directionRight3D = AngleAxis(FovAngleInDegrees, Vector3.up) * regimentForward;
            
            float2 directionLeft = directionLeft3D.xz;
            float2 directionRight = directionRight3D.xz;

            float2 intersection = GetIntersection(unit0, unitWidth, directionLeft, directionRight);
            float distanceIntersection = distance(intersection, unit0);
            Gizmos.DrawSphere(intersection.x1y(), 0.1f);
            
            (float arcAngle, float radius) = (FovAngleInDegrees * 2, RegimentType.Range + distanceIntersection);
            //ATTENTION: From exprime une direction et non une position => directionLeft.xOy() si x1y() le trait sera plus haut
            Handles.DrawWireArc(intersection.x1y(), Vector3.up, directionLeft.x0y(), arcAngle, radius, 2f);

            Vector3[] lines = GetLines(unit0, directionLeft, unitWidth, directionRight, RegimentType.Range);
            Handles.DrawLines(lines);

            GetBigTrianglePoints(directionLeft, directionRight);
            
            void GetBigTrianglePoints(float2 dirLeft, float2 dirRight)
            {
                float2 topForwardDirection = normalizesafe(regimentPosition - intersection);
                float2 topForwardFov = intersection + topForwardDirection * radius;

                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(topForwardFov.x1y(), 0.3f);
                
                float2 leftCross = topForwardDirection.CrossCounterClockWise();
                float2 intersectArcLeft = GetIntersection(topForwardFov, unit0, leftCross, dirLeft);
                
                float2 rightCross = topForwardDirection.CrossClockWise();
                float2 intersectArcRight = GetIntersection(topForwardFov, unitWidth, rightCross, dirRight);

                // DRAW Big Triangles
                float dst = distance(topForwardFov, intersectArcLeft);
                Gizmos.DrawRay(topForwardFov.x1y(), leftCross.x0y() * dst);
                Gizmos.DrawRay(topForwardFov.x1y(), rightCross.x0y() * dst);
                
                float dst2 = distance(unit0, intersectArcLeft);
                Gizmos.DrawRay(unit0.x1y(), dirLeft.x0y() * dst2);
                Gizmos.DrawRay(unitWidth.x1y(), dirRight.x0y() * dst2);
                
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(intersectArcLeft.x1y(), 0.2f);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(intersectArcRight.x1y(), 0.2f);
            }

            

            //Get Vision Cone Lines: true if you want to draw from intersection
            Vector3[] GetLines(float2 sLeft, float2 dirLeft, float2 sRight, float2 dirRight,float range, bool allCone = false)
            {
                if (!allCone)
                {
                    return new Vector3[] 
                    { 
                        unit0.x1y(), (sLeft + dirLeft * range).x1y(), //left Cone
                        unitWidth.x1y(), (sRight + dirRight * range).x1y(), //right Cone
                        unit0.x1y(), unitWidth.x1y() //first row line
                    };
                }
                else
                {
                    return new Vector3[]
                    {
                        intersection.x1y(), (sLeft + dirLeft * range).x1y(), //left Cone
                        intersection.x1y(), (sRight + dirRight * range).x1y(), //right Cone
                        unit0.x1y(), unitWidth.x1y() //first row line
                    };
                }
            }
        }

        private void DrawStartingFOV(in Vector3 unit0, in Vector3 unitWidth)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(unit0, 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(unitWidth, 0.2f);
        }
        
        private void Debug_FiringStateTargetDetection()
        {
            if (!FiringStateTest && StateMachine.State == EStates.Idle)
            {
                RegimentIdleState idleState = (RegimentIdleState)StateMachine.CurrentState;
                if (!idleState.AutoFire) return;
                idleState.AutoFireOff();
            }
            else if (FiringStateTest && StateMachine.State == EStates.Idle)
            {
                RegimentIdleState idleState = (RegimentIdleState)StateMachine.CurrentState;
                if (idleState.AutoFire) return;
                idleState.AutoFireOn();
            }
        }
    }
}
#endif