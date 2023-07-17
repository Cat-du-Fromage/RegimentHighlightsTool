using UnityEngine;

namespace KaizerWald
{
    public sealed class NullOrder<T> : Order<T>
    where T : MonoBehaviour
    {
        public NullOrder() : base(null, EStates.None) { }
        public sealed override bool IsNull => true;
    }
}