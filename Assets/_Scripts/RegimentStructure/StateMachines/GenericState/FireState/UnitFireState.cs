using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

using static KaizerWald.UnityMathematicsUtilities;
using static UnityEngine.LayerMask;
using static KaizerWald.KzwMath;
using static Unity.Mathematics.math;
using static Unity.Mathematics.int2;

using Random = Unity.Mathematics.Random;

using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

namespace KaizerWald
{
    public sealed class UnitFireState : FireState<Unit>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        //private const int MaxDownAngle = 20; Use to avoid Absurd AimDirection
        private Random randomState;
        private readonly UnitAnimation unitAnimation;
        
        private int maxRange;
        private int accuracy;
        //private float3 aimDirection;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public override Unit EnemyTarget { get; protected set; }
        public Regiment RegimentTargeted { get; private set; }
        public float3 AimDirection { get; private set; }
        public FormationData CacheEnemyFormation { get; private set; }
        
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
            maxRange = objectAttach.RegimentAttach.RegimentType.Range;
            accuracy = objectAttach.RegimentAttach.RegimentType.Accuracy;
            randomState = Random.CreateFromIndex((uint)(abs(objectAttach.GetInstanceID()) + objectAttach.IndexInRegiment));
        }
        
        
        public override void OnStateEnter(Order order)
        {
            //ResetDefaultValues();
            AttackOrder attackOrder = (AttackOrder)order;
            CacheEnemyFormation = attackOrder.TargetEnemyRegiment.CurrentFormation;
            RegimentTargeted = attackOrder.TargetEnemyRegiment;
            EnemyTarget = GetEnemyTarget();
            unitAnimation.SetFullFireSequenceOn();
        }

        public override void OnStateUpdate()
        {
            bool isUnitOnFirstLine = ObjectAttach.IndexInRegiment >= ObjectAttach.RegimentAttach.CurrentFormation.Width;
            if (isUnitOnFirstLine) return;
            
            //1) Est-ce que la formation adverse est changée?
            //si oui -> Retarget
            //sinon -> passer
            if (IsEnemyFormationChanged())
            {
                CacheEnemyFormation = RegimentTargeted.CurrentFormation;
                EnemyTarget = GetEnemyTarget();
            }
            else if(!IsTargetValid())
            {
                EnemyTarget = GetEnemyTarget();
            }
            UnitTakeAim();
            /*
            // L'unité doit réclamer? ou doit on attendre que le régiment le fasse
            if (IsTargetValid())
            {
                // Take Aim
                OnUnitAim();
                
                // Fire
                OnUnitShoot(); //TODO: il faudra empêcher de tirer sur les camarades
            }
            */
            
            //Issue target null may not be a good idea here..
            if (!OnTransitionCheck()) return;
            LinkedStateMachine.TransitionDefaultState();
        }

        public override bool OnTransitionCheck()
        {
            return EnemyTarget == null || EnemyTarget.IsDead;
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

        private bool IsEnemyFormationChanged()
        {
            return !CacheEnemyFormation.EqualComposition(RegimentTargeted.CurrentFormation);
        }
        
        private bool IsTargetValid()
        {
            return (EnemyTarget != null && !EnemyTarget.IsDead);
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

        private void UnitTakeAim()
        {
            if (unitAnimation.IsPlayingReload) return;
            float3 vectorUnitToTarget = TargetPosition - Position;
            float3 directionUnitToTarget = normalizesafe(vectorUnitToTarget);
            
            //Only on x and y axis (forward(z) axis dont have any value)
            float3 randomDirection = new (randomState.NextFloat2Direction() * (accuracy / 10f),0);
            float3 maxRangePosition = Position + maxRange * directionUnitToTarget;
            float3 spreadEndPoint = maxRangePosition + randomDirection;
            AimDirection = normalizesafe(spreadEndPoint - Position);

            if (unitAnimation.IsInFiringMode) return;
            unitAnimation.SetFullFireSequenceOn();
        }
        
        
        private void OnUnitAim()
        {
            if (unitAnimation.IsPlayingReload) return;
            float3 vectorUnitToTarget = TargetPosition - Position;
            //float distanceUnitToTarget = length(vectorUnitToTarget);
            float3 directionUnitToTarget = normalizesafe(vectorUnitToTarget);
            
            //Only on x and y axis (forward(z) axis dont have any value)
            float3 randomDirection = new (randomState.NextFloat2Direction() * (accuracy / 10f),0);
            float3 maxRangePosition = Position + maxRange * directionUnitToTarget;
            float3 spreadEndPoint = maxRangePosition + randomDirection;
            
            AimDirection = normalizesafe(spreadEndPoint - Position);
            unitAnimation.SetAimOn();
        }

        private void OnUnitShoot()
        {
            if (!unitAnimation.IsInFiringMode && unitAnimation.IsInAimingMode)
            {
                unitAnimation.SetFireOn();
            }
        }

        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Get Target Methods ◇◇◇◇◇◇                                                                          │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private Unit GetEnemyTarget()
        {
            float2 unitPosition = Position.xz;
            using NativeHashSet<int> hullIndices = GetUnitsHullIndices(CacheEnemyFormation);
            
            int index = -1;
            float minDistance = INFINITY;
            foreach (int unitIndex in hullIndices)
            {
                float2 enemyPosition = RegimentTargeted.UnitsTransform[unitIndex].position.xz();
                float distance = distancesq(unitPosition, enemyPosition);
                if (distance > minDistance) continue;
                (minDistance, index) = (distance, unitIndex);
            }
            
            return index == -1 ? null : RegimentTargeted.Units[index];
        }
        
        private NativeHashSet<int> GetUnitsHullIndices(in FormationData enemyFormation)
        {
            int numUnit = enemyFormation.NumUnitsAlive;
            int2 widthDepth = enemyFormation.WidthDepth;
            (int widthLastIndex, int depthLastIndex) = (widthDepth[0] - 1, widthDepth[1] - 1);

            NativeHashSet<int> indices = new (enemyFormation.NumCompleteLine * widthDepth[0], Temp);
            for (int i = 0; i < numUnit; i++)
            {
                int2 coord = GetXY2(i, widthDepth[0]);
                bool xyZero = any(coord == zero);
                bool xMaxWidth = coord.x == widthLastIndex;
                bool yMaxDepth = coord.y == depthLastIndex;
                if (!any(new bool3(xyZero, xMaxWidth, yMaxDepth))) continue; 
                indices.Add(i);
            }
            return indices;
        }
    }
}