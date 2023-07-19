using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using static UnityEngine.Vector2;

namespace KaizerWald
{
    public class RegimentOrder : Order
    {
        public static NullOrder Null { get; private set; } = new NullOrder();
        protected RegimentOrder(EStates stateOrdered): base(stateOrdered) { }
    }
}
