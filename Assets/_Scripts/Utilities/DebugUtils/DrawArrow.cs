using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using static UnityEngine.Quaternion;
using static UnityEngine.Vector3;
using static Unity.Mathematics.math;
using static Unity.Mathematics.quaternion;

namespace Kaizerwald
{
    /// Utility class for drawing arrows
    /// https://forum.unity.com/threads/debug-drawarrow.85980/
    public static class DrawArrow
    {
        private static Vector3 GetLeftArrowTip(in Vector3 startPosition, in Quaternion rotation, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Vector3 left = rotation * Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            return startPosition + left * arrowHeadLength;
        }
        
        private static Vector3 GetRightArrowDirection(in Vector3 startPosition, in Quaternion rotation, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Vector3 right = rotation * Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            return startPosition + right * arrowHeadLength;
        }
        
        public static void HandleLine(Vector3 pos1, Vector3 pos2, float offsetUp = 0, float thickness = 0f,float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            pos1 += Vector3.up * offsetUp;
            pos2 += Vector3.up * offsetUp;
            Handles.DrawLine(pos1, pos2, thickness);
            Vector3 direction = normalizesafe(pos2 - pos1);
            if (direction == zero) return;
            Quaternion rotation = LookRotationSafe(direction, up()); // LookRotation(direction);
            Handles.DrawLine(pos2, GetLeftArrowTip(pos2, rotation), thickness);
            Handles.DrawLine(pos2, GetRightArrowDirection(pos2, rotation), thickness);
        }

        public static void HandleLine(Vector3 pos1, Vector3 pos2, Color color, float offsetUp = 0, float thickness = 0f, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
            Color tmp = Handles.color;
            Handles.color = color;
            HandleLine(pos1, pos2,offsetUp,thickness, arrowHeadLength, arrowHeadAngle);
            Handles.color = tmp;
        }

        public static void GizmosLine(Vector3 pos1, Vector3 pos2, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawLine(pos1, pos2);
            Vector3 direction = normalizesafe(pos2 - pos1);
            if (direction == zero) return;
            Quaternion rotation = LookRotationSafe(direction, up()); // LookRotation(direction);
            //Vector3 right = rotation * Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward; //new Vector3(0, 0, 1);
            //Vector3 left = rotation * Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward; //new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos2, GetLeftArrowTip(pos2, rotation));
            Gizmos.DrawRay(pos2, GetRightArrowDirection(pos2, rotation));
        }

        public static void GizmosLine(Vector3 pos1, Vector3 pos2, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Color tmp = Gizmos.color;
            Gizmos.color = color;
            GizmosLine(pos1, pos2, arrowHeadLength, arrowHeadAngle);
            Gizmos.color = tmp;
        }

        public static void GizmosDirection(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);
            if (direction == zero) return;
            Quaternion rotation = LookRotationSafe(direction, up()); // LookRotation(direction);
            Vector3 right = rotation * Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward; //new Vector3(0, 0, 1);
            Vector3 left = rotation * Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward; //new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void GizmosDirection(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Color tmp = Gizmos.color;
            Gizmos.color = color;
            GizmosDirection(pos, direction, arrowHeadLength, arrowHeadAngle);
            Gizmos.color = tmp;
            /*
            Gizmos.DrawRay(pos, direction);
            Quaternion rotation = direction != Vector3.zero ? LookRotation(direction) : Quaternion.identity;

            Vector3 right = rotation * Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = rotation * Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
            */
        }

        public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction);
            Quaternion rotation = direction != Vector3.zero ? LookRotation(direction) : Quaternion.identity;
            Vector3 right = rotation * Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = rotation * Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            Debug.DrawRay(pos + direction, right * arrowHeadLength);
            Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }
        
        public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color);
            Quaternion rotation = direction != Vector3.zero ? LookRotation(direction) : Quaternion.identity;
            Vector3 right = rotation * Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = rotation * Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }
    }
}
