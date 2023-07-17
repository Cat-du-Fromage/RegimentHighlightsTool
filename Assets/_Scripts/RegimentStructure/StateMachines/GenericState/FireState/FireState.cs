using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class FireState<T> : State<T>
    where T : MonoBehaviour
    {
        public T EnemyTarget { get; protected set; }
        
        protected FireState(T objectAttach) : base(objectAttach, EStates.Fire)
        {
            EnemyTarget = null;
        }
    }
}
