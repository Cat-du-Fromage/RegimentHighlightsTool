using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using static UnityEngine.Vector2;

namespace KaizerWald
{
    public class RegimentOrder
    {
        public readonly EStates StateOrdered;
        public readonly Regiment Regiment;
        protected RegimentOrder(Regiment regiment,EStates stateOrdered)
        {
            Regiment = regiment;
            StateOrdered = stateOrdered;
        }
    }
}
