using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class UnitState
    {
        protected Unit UnitAttach;
        
        protected UnitState(Unit unit)
        {
            UnitAttach = unit;
        }

        public abstract void OnStateEnter();
        public abstract void OnOrderEnter(RegimentOrder order);
        public abstract void OnStateUpdate();
        public abstract bool OnTransitionCheck();
        public abstract void OnStateExit();
    }
}
