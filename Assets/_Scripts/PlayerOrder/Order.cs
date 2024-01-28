using UnityEngine;

namespace Kaizerwald
{
    public class Order
    {
        public static Order Default { get; private set; } = new Order(EStates.Idle);
        public static readonly Order Null = new Order(EStates.None);
        public EStates StateOrdered { get; protected set; }
        
        protected Order(EStates state)
        {
            StateOrdered = state;
        }
        public virtual bool IsNull => false;
    }
}