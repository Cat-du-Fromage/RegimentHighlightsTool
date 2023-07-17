using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using static Unity.Mathematics.math;

namespace KaizerWald
{
    
    public sealed class UnitMoveState : MoveState<Unit>
    {
        private readonly UnitAnimation unitAnimation;
        
        public UnitMoveState(Unit unit) : base(unit, unit.RegimentAttach.RegimentType.Speed)
        {
            Destination = ObjectTransform.position;
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

        public override void OnStateEnter(Order<Unit> order)
        {
            ResetDefaultValues();
            UnitMoveOrder moveOrder = (UnitMoveOrder)order;
            Direction = moveOrder.Direction;
            Destination = moveOrder.Destination;
            unitAnimation.SetMarching();
        }

        public override void OnStateUpdate()
        {
            float3 direction = normalizesafe(Destination - Position);
            float3 translation = Time.deltaTime * MoveSpeed * direction;
            ObjectTransform.Translate(translation, Space.World);
            ObjectTransform.LookAt(Position + Direction);
            
            if (!OnTransitionCheck()) return;
            LinkedStateMachine.TransitionState(EStates.Idle, UnitOrder.Null);
        }

        public override bool OnTransitionCheck()
        {
            //Pour le cas ou la vitesse nous ferait "dépasser" le point d'arriver
            //Penser a "cache" la dernière distance et comparer, Si magnitude calculer > dernière valeur => forcer la position
            return distancesq(Position,Destination) <= 0.01f;
        }

        public override void OnStateExit()
        {
            ResetDefaultValues();
        }
        
        public override void ResetDefaultValues()
        {
            SetMarching();
        }
    }
    
}
