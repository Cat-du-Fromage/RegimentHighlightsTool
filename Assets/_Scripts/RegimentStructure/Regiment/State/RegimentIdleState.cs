using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class RegimentIdleState : RegimentState
    {
        private bool autoFire = false;
        
        public RegimentIdleState(Regiment regiment) : base(regiment)
        {
            
        }

        public override void OnOrderEnter(RegimentOrder order)
        {
            
        }
        
        public override void OnStateEnter()
        {
            
        }

        public override void OnStateUpdate()
        {
            if(!autoFire) return;
            //Sinon Check Si Ennemis à portés
        }

        public override bool OnTransitionCheck()
        {
            return false;
        }

        public override void OnStateExit()
        {
            
        }
    }
}
