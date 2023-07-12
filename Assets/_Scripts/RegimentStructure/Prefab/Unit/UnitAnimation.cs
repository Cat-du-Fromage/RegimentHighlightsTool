using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
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
        
        public bool shoot, aim;
        //trigger
        private int animTriggerIDDeath;
        //int
        private int animIDDeathIndex;
        //Speeds
        private int animIDAnimationsSpeed, animIDSpeed, animIDIdleSpeed;
        //bool
        private int animIDIsAiming, animIDIsShooting;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void Awake()
        {
            InitializeComponents();
            AssignAnimationIDs();
        }

        private void Start()
        {
            //PROTOTYPE
            int regIndex = unitAttach.RegimentAttach == null ? 1 : unitAttach.RegimentAttach.GetInstanceID();
            InitializeIdleRandom(abs(GetInstanceID() + regIndex));
        }
        
        private void Update()
        {
            if (!Keyboard.current.eKey.wasPressedThisFrame) return;
            aim = !aim;
            animator.SetBool(animIDIsAiming, aim);
            shoot = !shoot;
            animator.SetBool(animIDIsShooting, shoot);
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

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
            aim = state;
            animator.SetBool(animIDIsAiming, aim);
            shoot = state;
            animator.SetBool(animIDIsShooting, shoot);
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