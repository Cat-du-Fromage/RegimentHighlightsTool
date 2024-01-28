#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    /*
    public partial class Unit : MonoBehaviour
    {
        private bool DebugAvailable = false;
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            Debug_FireState();
        }

        private void Debug_FireState()
        {
            if (!StateMachine.IsFiring) return;
            FireUnitState fireState = (FireUnitState)StateMachine.CurrentState;
            ShowGizmosTarget(fireState);
            ShowAimTarget(fireState);
        }
        
        private void ShowGizmosTarget(FireUnitState fireState)
        {
            if (!RegimentAttach.ShowTargetsFiringStateTest) return;
            if(fireState.RegimentTargeted == null) return;
            Vector3 unitPosition = transform.position;
            Vector3 targetPosition = fireState.TargetPosition;
            DrawArrow.HandleLine(unitPosition, targetPosition, Color.red,1f, 0.5f);
        }
        
        private void ShowAimTarget(FireUnitState fireState)
        {
            if (!Animation.IsInAimingMode) return;
            if(fireState.RegimentTargeted == null) return;
            Vector3 unitPosition = transform.position;
            Vector3 directionTarget = fireState.AimDirection;
            
            float distanceUnitToTarget = Vector3.Distance(unitPosition, fireState.TargetPosition);

            Vector3 endPosition = unitPosition + directionTarget * distanceUnitToTarget;
            DrawArrow.HandleLine(unitPosition, endPosition, Color.magenta,1f, 0.5f);
        }
    }
    */
}
#endif