using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using static UnityEngine.Vector2;

namespace KaizerWald
{
    public class RegimentOrder : Order<Regiment>
    {
        public static NullOrder<Regiment> Null { get; private set; } = new NullOrder<Regiment>();
        protected RegimentOrder(Regiment regiment,EStates stateOrdered): base(regiment, stateOrdered)
        {
            
        }
    }
}
