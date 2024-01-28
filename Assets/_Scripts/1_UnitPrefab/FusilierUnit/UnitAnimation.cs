using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

using static UnityEngine.Animator;
using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

namespace Kaizerwald
{
    //TODO: FIND BETTER SOLUTION...
    public enum EUnitAnimation
    {
        RifleIdle,
        WalkRifle,
        RunRifle,
        RifleDownToAim,
        RifleAimingIdle,
        RifleAimToDown,
        FusilierFiring,
        RifleReloading,
        RifleFrontDeath0,
        RifleFrontDeath1,
        RifleFrontDeath2,
    }
    
    public partial class UnitAnimation : MonoBehaviour
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
        
        public event Action<AnimationEvent> OnShootEvent;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [SerializeField] private Dictionary<string, AnimationClip> AnimationClips;

        public bool IsInAimingMode => aim;
        public bool IsInFiringMode => aim && shoot;
        
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
        }

        private void Start()
        {
            int regIndex = unitAttach.RegimentAttach == null ? 1 : unitAttach.RegimentAttach.GetInstanceID();
            InitializeIdleRandom(abs(GetInstanceID() + regIndex));
            
            GetAllCLips();
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
            SetFullFireSequence(!IsInFiringMode);
        }


        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Current State Accessor ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private string GetCurrentClipName => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        
        public bool IsPlaying(EUnitAnimation unitAnimation) => GetCurrentClipName == unitAnimation.ToString();
        
        public bool IsPlayingIdle => GetCurrentClipName == nameof(EUnitAnimation.RifleIdle);
        public bool IsPlayingFire => GetCurrentClipName == nameof(EUnitAnimation.FusilierFiring);
        public bool IsPlayingReload => GetCurrentClipName == nameof(EUnitAnimation.RifleReloading);

        //TODO: Since unit share all the same animation, We need to Moveit to UnitManager (at least the way to retrieve them)
        private void GetAllCLips()
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            AnimationClips = new Dictionary<string, AnimationClip>(clips.Length);
            clips.ForEach(clip => AnimationClips.Add(clip.name, clip));
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
            speedIdle = rand.NextFloat(4, 11) / 10f;
            animator.SetFloat(animIDIdleSpeed, speedIdle);
            animationsSpeed = rand.NextFloat(6, 11) / 10f;
            animator.SetFloat(animIDAnimationsSpeed, animationsSpeed);
            int randomDeathIndex = rand.NextInt(0, 3);
            animator.SetInteger(animIDDeathIndex, randomDeathIndex); //Random.CreateFromIndex((uint)index).NextInt(0, 3);
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Animation Triggers ◈◈◈◈◈◈                                                                      ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ March/Run ◇◇◇◇◇◇                                                                                   │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void SetSpeed(float value)
        {
            animator.SetFloat(animIDSpeed, value);
        }
        public void SetIdle() => SetSpeed(0);
        public void SetMarching() => SetSpeed(2);
        public void SetRunning() => SetSpeed(6);
        

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Aim ◇◇◇◇◇◇                                                                                         │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

        private void SetAim(bool state) => animator.SetBool(animIDIsAiming, aim = state);
        public void SetAimOn() => SetAim(true);
        public void SetAimOff() => SetAim(false);
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Fire ◇◇◇◇◇◇                                                                                        │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void SetFire(bool state) => animator.SetBool(animIDIsShooting, shoot = state);
        public void SetFireOn() => SetFire(true);
        public void SetFireOff() => SetFire(false);
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Aim + FIre ◇◇◇◇◇◇                                                                                  │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void SetFullFireSequence(bool state)
        {
            if (state) SetSpeed(0);
            animator.SetBool(animIDIsAiming, aim = state);//aim = state;
            animator.SetBool(animIDIsShooting, shoot = state);//shoot = state;
        }
        public void SetFullFireSequenceOn() => SetFullFireSequence(true);
        public void SetFullFireSequenceOff() => SetFullFireSequence(false);

        public void SetDead()
        {
            animator.SetTrigger(animTriggerIDDeath);
        }

        public void OnShootTrigger(AnimationEvent animationEvent)
        {
            OnShootEvent?.Invoke(animationEvent);
            MuzzleFlash.Play();
            //Debug.Log($"test: {animationEvent.animatorClipInfo.clip.name}");
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Test To Get Time ◇◇◇◇◇◇                                                                            │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        /*
        private float GetCurrentAnimatorTime()
        {
            AnimatorStateInfo animationState = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo[] myAnimatorClip = animator.GetCurrentAnimatorClipInfo(0);
            float myTime = myAnimatorClip[0].clip.length * animationState.normalizedTime;
            return myTime;
        }
        */
    }
}