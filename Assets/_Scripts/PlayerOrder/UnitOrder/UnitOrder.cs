using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class UnitOrder : Order
    {
        public static NullOrder Null { get; private set; } = new NullOrder();
        
        //ajouter IdleOrder Comme Default!
        
        protected UnitOrder(EStates state) : base(state) { }
        protected UnitOrder(RegimentOrder baseOrder) : base(baseOrder.StateOrdered){ }
    }
}
