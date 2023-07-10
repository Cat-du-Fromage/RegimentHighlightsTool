using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class UnitMoveState : UnitState
    {
        private float speed;
        private Transform unitTransform;
        public Vector3 Destination { get; private set; }
        public Vector3 Position => unitTransform.position;
        //public Vector2 position2D => new Vector2(unitTransform.position.x, unitTransform.position.z);
        public UnitMoveState(Unit unit) : base(unit)
        {
            speed = unit.RegimentAttach.RegimentType.Speed;
            unitTransform = unit.transform;
            Destination = unitTransform.position;
        }

        public override void OnStateEnter()
        {
            return;
        }

        public override void OnOrderEnter(RegimentOrder order)
        {
            MoveRegimentOrder moveOrder = (MoveRegimentOrder)order;
            Destination = moveOrder.FormationDestination.GetUnitRelativePositionToRegiment3D(UnitAttach.IndexInRegiment, moveOrder.LeaderDestination);
        }

        public override void OnStateUpdate()
        {
            Vector3 direction = (Destination - Position).normalized;
            unitTransform.Translate(Time.deltaTime * speed * direction);

            if (!OnTransitionCheck()) return;
            UnitAttach.TransitionState(EStates.Idle);
        }

        public override bool OnTransitionCheck()
        {
            //Pour le cas ou la vitesse nous ferait "dépasser" le point d'arriver
            //Penser a "cache" la dernière distance et comparer, Si magnitude calculer > dernière valeur => forcer la position
            return (Destination - Position).magnitude <= 0.01f;
        }

        public override void OnStateExit()
        {
            return;
        }
    }
}
