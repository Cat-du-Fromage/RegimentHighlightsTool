using UnityEngine;

namespace KaizerWald
{
    public class IdleState<T> : State<T>
    where T : MonoBehaviour
    {
        public IdleState(T objectAttach) : base(objectAttach, EStates.Idle)
        {
        }
    }
}