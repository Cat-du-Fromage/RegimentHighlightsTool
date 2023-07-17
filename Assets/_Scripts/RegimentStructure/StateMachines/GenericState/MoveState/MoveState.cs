using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
        public float3 Destination { get; protected set; }
        public float3 Direction { get; protected set; }
        public bool IsRunning { get; private set; }

        public MoveState(T objectAttach, float speed = 1f) : base(objectAttach, EStates.Idle)
        {
            Speed = speed;
            SpeedModifier = 1;
        }

        protected float MoveSpeed => Speed * SpeedModifier;
        protected float3 Position => ObjectTransform.position;

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
        // fonction similaire pour le moment mais le final rendra le comportement de Regiment et Unit très différent
    }
}
