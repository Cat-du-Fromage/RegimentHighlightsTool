using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public sealed class IdleOrder : Order
    {
        public IdleOrder() : base(EStates.Idle) { }
    }
}
