using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

using static UnityEngine.Animator;
using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

namespace KaizerWald
{
    public class UnitAnimation : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [SerializeField] private ParticleSystem MuzzleFlash;
        [SerializeField] private float animationsSpeed, speedIdle, speed;
        
        private Unit unitAttach;
        private Animator animator;
        
        //can aim but stay Idle!
        public bool shoot, aim;
        //trigger
        private int animTriggerIDDeath;
        //int: choose which animation to play based on Id
        private int animIDDeathIndex;
        //Speeds
        private int animIDAnimationsSpeed, animIDSpeed, animIDIdleSpeed;
        //bool: enable/disable animation
        private int animIDIsAiming, animIDIsShooting;

        public bool IsAiming => aim;
        public bool IsFiring => aim && shoot;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
#if UNITY_EDITOR
        private void Reset()
        {
            MuzzleFlash = MuzzleFlash == null ? GetComponentInChildren<ParticleSystem>() : MuzzleFlash;
        }
#endif
        
        private void Awake()
        {
            InitializeComponents();
            AssignAnimationIDs();
            int regIndex = unitAttach.RegimentAttach == null ? 1 : unitAttach.RegimentAttach.GetInstanceID();
            InitializeIdleRandom(abs(GetInstanceID() + regIndex));
        }

        private void Update()
        {
            ToggleFireAnimation();
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void ToggleFireAnimation()
        {
            if (!Keyboard.current.eKey.wasPressedThisFrame) return;
            SetFire(!IsFiring);
            //FIRST: change value of shoot/aim THEN: use the newly changed value to SetBool
            //animator.SetBool(animIDIsAiming, aim = !aim); //aim = !aim;
            //animator.SetBool(animIDIsShooting, shoot = !shoot); //shoot = !shoot;
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void InitializeComponents()
        {
            unitAttach  = GetComponent<Unit>();
            animator    = GetComponent<Animator>();
            MuzzleFlash = MuzzleFlash == null ? GetComponentInChildren<ParticleSystem>() : MuzzleFlash;
        }
        
        private void AssignAnimationIDs()
        {
            animTriggerIDDeath    = StringToHash("Death");
            animIDDeathIndex      = StringToHash("DeathIndex");
            animIDAnimationsSpeed = StringToHash("AnimationsSpeed");
            animIDSpeed           = StringToHash("Speed");
            animIDIsAiming        = StringToHash("IsAiming");
            animIDIsShooting      = StringToHash("IsShooting");
            animIDIdleSpeed       = StringToHash("IdleSpeed");
        }
        
        public void InitializeIdleRandom(int index)
        {
            Random rand = Random.CreateFromIndex(min((uint)index, uint.MaxValue - 1));
            speedIdle = rand.NextInt(4, 11) / 10f;
            animator.SetFloat(animIDIdleSpeed, speedIdle);
            animationsSpeed = rand.NextInt(6, 11) / 10f;
            animator.SetFloat(animIDAnimationsSpeed, animationsSpeed);
            animator.SetInteger(animIDDeathIndex, Random.CreateFromIndex((uint)index).NextInt(0,3));
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Animation Triggers ◈◈◈◈◈◈                                                                      ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void SetSpeed(float value)
        {
            animator.SetFloat(animIDSpeed, value);
        }

        public void SetMarching() => SetSpeed(2);
        public void SetRunning() => SetSpeed(6);

        public void SetFire(bool state)
        {
            animator.SetBool(animIDIsAiming, aim = state);//aim = state;
            animator.SetBool(animIDIsShooting, shoot = state);//shoot = state;
        }

        public void SetDead()
        {
            animator.SetTrigger(animTriggerIDDeath);
        }

        private void PlayMuzzleFlash()
        {
            MuzzleFlash.Play();
        }
    }
}