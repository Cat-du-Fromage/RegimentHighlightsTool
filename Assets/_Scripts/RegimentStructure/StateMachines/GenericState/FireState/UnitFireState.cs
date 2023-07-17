using UnityEngine;
using Unity.Mathematics;

using static KaizerWald.UnityMathematicsUtilities;
using static UnityEngine.LayerMask;
using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

namespace KaizerWald
{
    public sealed class UnitFireState : FireState<Unit>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        //private const int MaxDownAngle = 20; Use to avoid Absurd AimDirection
        private Random randomState;
        
        private readonly int MaxRange;
        private readonly int Accuracy;
        
        private readonly LayerMask unitLayerMask;
        private readonly UnitAnimation unitAnimation;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public float3 AimDirection { get; private set; }
        
        private float3 TargetPosition => EnemyTarget.transform.position;
        private UnitStateMachine LinkedUnitStateMachine => ObjectAttach.StateMachine;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public UnitFireState(Unit objectAttach) : base(objectAttach)
        {
            objectAttach.Animation.OnShootEvent += OnFireEvent;
            
            EnemyTarget = null;
            unitAnimation = objectAttach.Animation;
            unitLayerMask = RegimentManager.Instance == null ? NameToLayer("Unit") : RegimentManager.Instance.UnitLayerMask;
            
            MaxRange = objectAttach.RegimentAttach.RegimentType.Range;
            Accuracy = objectAttach.RegimentAttach.RegimentType.Accuracy;
            
            randomState = Random.CreateFromIndex((uint)(abs(objectAttach.GetInstanceID()) + objectAttach.IndexInRegiment));
        }
        
        
        public override void OnStateEnter(Order<Unit> order)
        {
            ResetDefaultValues();
            AttackUnitOrder attackOrder = (AttackUnitOrder)order;
            EnemyTarget = attackOrder.EnemyTarget;
        }

        public override void OnStateUpdate()
        {
            bool isUnitOnFirstLine = ObjectAttach.IndexInRegiment >= ObjectAttach.RegimentAttach.CurrentFormation.Width;
            if (isUnitOnFirstLine) return;
            
            // L'unité doit réclamer? ou doit on attendre que le régiment le fasse
            CheckAndRequestTarget();
            // Take Aim
            OnUnitAim();
            // Fire
            OnUnitShoot(); //TODO: il faudra empêcher de tirer sur les camarades

            //Issue target null may not be a good idea here..
            if (!OnTransitionCheck()) return;
            LinkedStateMachine.TransitionDefaultState();
        }

        public override bool OnTransitionCheck()
        {
            return EnemyTarget == null;
        }

        public override void OnStateExit()
        {
            ResetDefaultValues();
        }

        public override void ResetDefaultValues()
        {
            unitAnimation.SetAimOff();
            unitAnimation.SetFireOff();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void CheckAndRequestTarget()
        {
            if (EnemyTarget != null && !EnemyTarget.IsDead) return; 
            LinkedUnitStateMachine.OnStateRequest(StateIdentity);
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Animation Event ◈◈◈◈◈◈                                                                         ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        // Call : objectAttach.Animation.OnShootEvent
        private void OnFireEvent(AnimationEvent animationEvent)
        {
            float3 bulletPosition = Position + up() + (float3)ObjectTransform.forward;
            ProjectileComponent bullet = ProjectileManager.Instance.UnitRequestAmmo(ObjectAttach, bulletPosition);
            bullet.Fire(bulletPosition,AimDirection);
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Shoot Sequence ◈◈◈◈◈◈                                                                          ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void OnUnitAim()
        {
            if (unitAnimation.IsReloading) return;
            float3 vectorUnitToTarget = TargetPosition - Position;
            //float distanceUnitToTarget = length(vectorUnitToTarget);
            float3 directionUnitToTarget = normalizesafe(vectorUnitToTarget);
            
            //Only on x and y axis (forward(z) axis dont have any value)
            float3 randomDirection = new float3(randomState.NextFloat2Direction() * (Accuracy / 10f),0);
            float3 maxRangePosition = Position + MaxRange * directionUnitToTarget;
            float3 spreadEndPoint = maxRangePosition + randomDirection;
            
            AimDirection = normalizesafe(spreadEndPoint - Position);
            unitAnimation.SetAimOn();
        }

        private void OnUnitShoot()
        {
            if (!unitAnimation.IsFiring && unitAnimation.IsAiming)
            {
                unitAnimation.SetFireOn();
            }
        }

    }
}