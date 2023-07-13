using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace KaizerWald
{
    /*
    public partial class RegimentStateMachine : StateMachine<Regiment>
    {
        private int[] DebugOrderedIndices;
        private void OnDrawGizmos()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (State != EStates.Move) return;

            RegimentMoveState moveState = (RegimentMoveState)CurrentState;
            DebugOrderedIndices = moveState.GetIndicesOrderedByFurthestLineThenMiddle(ObjectAttach.CurrentFormation);
            FormationData formationData = moveState.FormationDestination;
            Vector2[] positions = formationData.GetUnitsPositionRelativeToRegiment(moveState.Destination);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(moveState.Destination, 0.5f);
            Gizmos.color = Color.cyan;
            for (int i = 0; i < DebugOrderedIndices.Length; i++)
            {
                Vector3 position = new Vector3(positions[i].x, 1, positions[i].y);
                Gizmos.DrawWireSphere(position * new float3(1,0,1), 0.5f);
                Handles.Label(position, i.ToString());
            }
        }
    }
    */
}
#endif