using UnityEngine;

namespace KaizerWald
{
    public class Order
    {
        public static Order Default { get; private set; } = new Order(EStates.Idle);
        public static Order Null { get; private set; } = new Order(EStates.None);
        public EStates StateOrdered { get; protected set; }
        
        protected Order(EStates state)
        {
            StateOrdered = state;
        }
        public virtual bool IsNull => false;
    }
}