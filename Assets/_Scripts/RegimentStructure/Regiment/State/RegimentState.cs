using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class RegimentState
    {
        protected Regiment RegimentAttach;
        
        protected RegimentState(Regiment regiment)
        {
            RegimentAttach = regiment;
        }

        public abstract void OnStateEnter();
        public abstract void OnOrderEnter(RegimentOrder order);
        public abstract void OnStateUpdate();
        public abstract bool OnTransitionCheck();
        public abstract void OnStateExit();
    }
}
