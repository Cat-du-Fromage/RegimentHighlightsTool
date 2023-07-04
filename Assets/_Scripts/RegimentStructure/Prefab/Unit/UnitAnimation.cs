using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

namespace KaizerWald
{
    public class UnitAnimation : MonoBehaviour
    {
        private Unit unitAttach;

        [SerializeField] private ParticleSystem MuzzleFlash;
        private Animator animator;

        public float animationsSpeed;
        public float speedIdle;
        public float speed;

        public bool shoot;
        public bool aim;

        //trigger
        private int animTriggerIDDeath;

        //int
        private int animIDDeathIndex;

        //float
        private int animIDAnimationsSpeed;
        private int animIDSpeed;

        private int animIDIdleSpeed;

        //bool
        private int animIDIsAiming;
        private int animIDIsShooting;

        private void Awake()
        {
            unitAttach = GetComponent<Unit>();
            animator = GetComponent<Animator>();
            AssignAnimationIDs();

            MuzzleFlash = MuzzleFlash == null ? GetComponentInChildren<ParticleSystem>() : MuzzleFlash;
        }

        private void Start()
        {
            //PROTOTYPE
            int regIndex = unitAttach.RegimentAttach == null ? 1 : unitAttach.RegimentAttach.RegimentID;
            InitIdleRandom(abs(unitAttach.IndexInRegiment + regIndex));
        }

        public void SetSpeed(float value)
        {
            animator.SetFloat(animIDSpeed, value);
        }

        private void AssignAnimationIDs()
        {
            animTriggerIDDeath = Animator.StringToHash("Death");

            animIDDeathIndex = Animator.StringToHash("DeathIndex");

            animIDAnimationsSpeed = Animator.StringToHash("AnimationsSpeed");
            animIDSpeed = Animator.StringToHash("Speed");

            animIDIsAiming = Animator.StringToHash("IsAiming");
            animIDIsShooting = Animator.StringToHash("IsShooting");
            animIDIdleSpeed = Animator.StringToHash("IdleSpeed");
        }

        public void InitIdleRandom(int index)
        {
            Random rand = Random.CreateFromIndex(min((uint)index, uint.MaxValue - 1));

            speedIdle = rand.NextInt(4, 11) / 10f;
            animator.SetFloat(animIDIdleSpeed, speedIdle);

            animationsSpeed = rand.NextInt(6, 11) / 10f;
            animator.SetFloat(animIDAnimationsSpeed, animationsSpeed);
            
            animator.SetInteger(animIDDeathIndex, Random.CreateFromIndex((uint)index).NextInt(0,3));
        }

        private void Update()
        {
            if (!Keyboard.current.eKey.wasPressedThisFrame) return;
            aim = !aim;
            animator.SetBool(animIDIsAiming, aim);

            shoot = !shoot;
            animator.SetBool(animIDIsShooting, shoot);
        }

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