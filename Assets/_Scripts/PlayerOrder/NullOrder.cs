using UnityEngine;

namespace Kaizerwald
{
    public sealed class NullOrder : Order
    {
        public NullOrder() : base(EStates.None) { }
        public sealed override bool IsNull => true;
    }
}