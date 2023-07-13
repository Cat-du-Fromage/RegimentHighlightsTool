using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace KaizerWald
{
    public class MoveState<T> : State<T>
    where T : MonoBehaviour
    {
        protected readonly float Speed;
        protected int SpeedModifier;
        
        protected readonly Transform ObjTransform;
        public Vector3 Destination { get; protected set; }
        public Vector3 Direction { get; protected set; }
        public bool IsRunning { get; private set; }

        public MoveState(T objectAttach, float speed = 1f) : base(objectAttach)
        {
            ObjTransform = objectAttach.transform;
            Speed = speed;
            SpeedModifier = 1;
        }

        public float MoveSpeed => Speed * SpeedModifier;
        protected Vector3 Position => ObjTransform.position;

        public virtual void SetRunning()
        {
            SpeedModifier = 3;
            IsRunning = true;
        }

        public virtual void SetMarching()
        {
            SpeedModifier = 1;
            IsRunning = false;
        }

        public override void OnAbilityTrigger()
        {
            if (IsRunning)
            {
                SetMarching();
                return;
            }
            SetRunning();
        }
        
        public override void OnStateEnter() { return; }
        public override void OnOrderEnter(RegimentOrder order) { return; }
        public override void OnStateUpdate() { return; }
        public override bool OnTransitionCheck() { return false; }
        public override void OnStateExit() { return; }
    }
}
