#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public partial class Unit : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            Debug_FireState();
        }

        private void Debug_FireState()
        {
            if (!StateMachine.IsFiring) return;
            UnitFireState fireState = (UnitFireState)StateMachine.CurrentState;
            ShowGizmosTarget(fireState);
            ShowAimTarget(fireState);
        }
        
        private void ShowGizmosTarget(UnitFireState fireState)
        {
            if (!RegimentAttach.ShowTargetsFiringStateTest) return;
            if(fireState.EnemyTarget == null) return;
            Vector3 unitPosition = transform.position;
            Vector3 targetPosition = fireState.EnemyTarget.transform.position;
            DrawArrow.HandleLine(unitPosition, targetPosition, Color.red,1f, 0.5f);
        }
        
        private void ShowAimTarget(UnitFireState fireState)
        {
            if (!Animation.IsAiming) return;
            if(fireState.EnemyTarget == null) return;
            Vector3 unitPosition = transform.position;
            Vector3 directionTarget = fireState.AimDirection;
            
            float distanceUnitToTarget = Vector3.Distance(unitPosition, fireState.EnemyTarget.transform.position);

            Vector3 endPosition = unitPosition + directionTarget * distanceUnitToTarget;
            DrawArrow.HandleLine(unitPosition, endPosition, Color.magenta,1f, 0.5f);
        }
    }
}
#endif