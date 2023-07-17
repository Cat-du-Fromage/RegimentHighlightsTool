using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class UnitOrder : Order<Unit>
    {
        public static NullOrder<Unit> Null { get; private set; } = new NullOrder<Unit>();

        protected UnitOrder(Unit receiver, EStates state) : base(receiver, state)
        {
        }
        
        protected UnitOrder(Unit receiver, RegimentOrder baseOrder) : base(receiver, baseOrder.StateOrdered)
        {
        }
    }
}
