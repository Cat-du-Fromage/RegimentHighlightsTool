using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class RegimentMoveState : RegimentState
    {
        private float speed;
        private Transform regimentTransform;
        public Vector3 LeaderDestination { get; private set; }
        public FormationData FormationDestination { get; private set; }
        
        public RegimentMoveState(Regiment regiment) : base(regiment)
        {
            speed = regiment.RegimentType.Speed;
            regimentTransform = regiment.transform;
            LeaderDestination = regimentTransform.position;
            FormationDestination = regiment.CurrentFormation;
        }

        public override void OnStateEnter()
        {
            
        }
        
        public override void OnOrderEnter(RegimentOrder order)
        {
            MoveRegimentOrder moveOrder = (MoveRegimentOrder)order;
            LeaderDestination = moveOrder.LeaderDestination;
            FormationDestination = moveOrder.FormationDestination;

            //RegimentAttach.CurrentFormation.SetWidth(FormationDestination.Width);
            //RegimentAttach.CurrentFormation.SetDirection(FormationDestination.Direction2DForward);
        }

        public override void OnStateUpdate()
        {
            //Update Leader
            //TODO: Update formation one by one
            RegimentAttach.CurrentFormation.SetWidth(FormationDestination.Width);
            //RegimentAttach.CurrentFormation.SetDirection(FormationDestination.Direction2DForward);
            
            Vector3 direction = (LeaderDestination - regimentTransform.position).normalized;
            regimentTransform.Translate(Time.deltaTime * speed * direction);
            regimentTransform.LookAt(regimentTransform.position + (Vector3)FormationDestination.Direction3DForward());
            if (!OnTransitionCheck()) return;
            RegimentAttach.TransitionState(EStates.Idle);
        }

        public override bool OnTransitionCheck()
        {
            return (LeaderDestination - regimentTransform.position).magnitude <= 0.01f;
        }

        public override void OnStateExit()
        {
            
        }
    }
}
