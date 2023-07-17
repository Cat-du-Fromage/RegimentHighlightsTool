using UnityEngine;

namespace KaizerWald
{
    public class Order<T>
    where T : MonoBehaviour
    {
        public readonly T Receiver;
        public readonly EStates StateOrdered;
        protected Order(T receiver, EStates state)
        {
            Receiver = receiver;
            StateOrdered = state;
        }
        public virtual bool IsNull => false;
    }
}