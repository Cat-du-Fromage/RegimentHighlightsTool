using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KaizerWald
{
    [CustomEditor(typeof (RegimentFieldOfView))]
    public class FieldOfViewEditor : Editor
    {
        private Vector3 currentPosition;
        
        private void OnSceneGUI() 
        {
            RegimentFieldOfView fow = (RegimentFieldOfView)target;
            DrawFieldOfView(fow);
        }

        private void DrawFieldOfView(RegimentFieldOfView fow)
        {
            currentPosition = fow.transform.position;
            
            Handles.color = Color.white;
            Handles.DrawWireArc (currentPosition, Vector3.up, Vector3.forward, fow.ViewAngle, fow.ViewRadius);
            Vector3 viewAngleA = fow.GetDirectionFromAngle(-fow.ViewAngle / 2, false);
            Vector3 viewAngleB = fow.GetDirectionFromAngle(fow.ViewAngle / 2, false);
            
            Handles.DrawLine (currentPosition, currentPosition + viewAngleA * fow.ViewRadius);
            Handles.DrawLine (currentPosition, currentPosition + viewAngleB * fow.ViewRadius);
        }
    }
}
