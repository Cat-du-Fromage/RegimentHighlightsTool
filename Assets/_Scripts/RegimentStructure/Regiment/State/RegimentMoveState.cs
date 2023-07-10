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
        }

        public override void OnStateUpdate()
        {
            //Update Leader
            //TODO: Update formation one by one
            RegimentAttach.CurrentFormation.SetWidth(FormationDestination.Width);
            
            Vector3 direction = (LeaderDestination - regimentTransform.position).normalized;
            regimentTransform.Translate(Time.deltaTime * speed * direction);

            if (!OnTransitionCheck()) return;
            RegimentAttach.TransitionState(new RegimentIdleState(RegimentAttach));
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
