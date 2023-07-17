using UnityEngine;

namespace KaizerWald
{
    public class NullState<T> : State<T>
    where T : MonoBehaviour
    {
        protected NullState(T objectAttach) : base(objectAttach, EStates.None) { }
        public sealed override bool IsNull => true;
    }
}