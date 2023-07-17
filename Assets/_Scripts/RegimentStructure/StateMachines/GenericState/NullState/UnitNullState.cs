using System.Collections;
using System.Collections.Generic;

namespace KaizerWald
{
    public sealed class UnitNullState : UnitState
    {
        public UnitNullState(Unit objectAttach = null) : base(objectAttach, EStates.None, RegimentState.Null) { }
    }
}
