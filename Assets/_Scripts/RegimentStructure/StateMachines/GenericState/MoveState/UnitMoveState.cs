using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace KaizerWald
{
    
    public sealed class UnitMoveState : MoveState<Unit>
    {
        private readonly UnitAnimation unitAnimation;
        
        public UnitMoveState(Unit unit) : base(unit, unit.RegimentAttach.RegimentType.Speed)
        {
            Destination = ObjTransform.position;

            if (!unit.TryGetComponent(out unitAnimation))
            {
                Debug.LogError($"Unit: {unit.name} don't have 'UnitAnimation' Component");
            }
        }

        public override void SetMarching()
        {
            base.SetMarching();
            unitAnimation.SetMarching();
        }

        public override void SetRunning()
        {
            base.SetRunning();
            unitAnimation.SetRunning();
        }
        
        public override void OnStateEnter()
        {
            ResetDefaultValues();
        }

        public override void OnOrderEnter(RegimentOrder order)
        {
            ResetDefaultValues();
            MoveRegimentOrder moveOrder = (MoveRegimentOrder)order;
            FormationData formationData = moveOrder.FormationDestination;
            Direction = formationData.Direction3DForward;
            Destination = formationData.GetUnitRelativePositionToRegiment3D(ObjectAttach.IndexInRegiment, moveOrder.LeaderDestination);
            unitAnimation.SetMarching();
        }

        public override void OnStateUpdate()
        {
            ObjTransform.LookAt(Position + Direction);
            Vector3 direction = (Destination - Position).normalized;
            ObjTransform.Translate(Time.deltaTime * MoveSpeed * direction, Space.World);
            if (!OnTransitionCheck()) return;
            
            ObjectAttach.StateMachine.TransitionState(EStates.Idle);
        }

        public override bool OnTransitionCheck()
        {
            //Pour le cas ou la vitesse nous ferait "dépasser" le point d'arriver
            //Penser a "cache" la dernière distance et comparer, Si magnitude calculer > dernière valeur => forcer la position
            return (Destination - Position).magnitude <= 0.01f;
        }

        public override void OnStateExit()
        {
            ResetDefaultValues();
        }
        
        private void ResetDefaultValues()
        {
            SetMarching();
        }
    }
    
}
