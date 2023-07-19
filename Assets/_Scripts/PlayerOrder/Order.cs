using UnityEngine;

namespace KaizerWald
{
    public class Order
    {
        public EStates StateOrdered { get; protected set; }
        
        protected Order(EStates state)
        {
            StateOrdered = state;
        }
        public virtual bool IsNull => false;
    }
}