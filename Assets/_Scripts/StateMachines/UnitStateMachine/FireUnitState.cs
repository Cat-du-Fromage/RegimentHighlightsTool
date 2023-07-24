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
    public class FireUnitState : UnitState
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Unit EnemyTarget { get; private set; }
        public Regiment RegimentTargeted { get; private set; }
        public FormationData CacheEnemyFormation { get; private set; }
        
        public float2 CurrentRandomAimDirection { get; private set; }
        public float3 AimDirection { get; private set; }
        
        //ATTENTION PAS { get; private set; } sinon NextFloat2Direction ne bougera pas
        private Random randomState;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public float3 TargetPosition => EnemyTarget.transform.position;
        private FormationData CurrentEnemyFormation => RegimentTargeted.CurrentFormation;
        private UnitAnimation UnitAnimation => UnitAttach.Animation;
        private int MaxRange => UnitAttach.RegimentAttach.RegimentType.Range;
        private int Accuracy => UnitAttach.RegimentAttach.RegimentType.Accuracy;
        
        private int IndexInRegiment => UnitAttach.IndexInRegiment;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public FireUnitState(UnitStateMachine linkedUnitStateMachine) : base(linkedUnitStateMachine, EStates.Fire)
        {
            randomState = Random.CreateFromIndex((uint)(abs(UnitAttach.GetInstanceID()) + UnitAttach.IndexInRegiment));
            CurrentRandomAimDirection = randomState.NextFloat2Direction();
            
            UnitAnimation.OnShootEvent += OnFireEvent;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override bool ConditionStateEnter()
        {
            bool check = IndexInRegiment < UnitAttach.RegimentAttach.CurrentFormation.Width;
            return check;
        }
        
        public override void SetupState(Order order)
        {
            AttackOrder attackOrder = (AttackOrder)order;
            RegimentTargeted = attackOrder.TargetEnemyRegiment;
            CacheEnemyFormation = attackOrder.TargetEnemyRegiment.CurrentFormation;
        }
        
        public override void EnterState()
        {
            if(!TryGetEnemyTarget(out Unit target)) return;
            EnemyTarget = target;
            UnitAnimation.SetFullFireSequenceOn();
        }
        

        public override void UpdateState()
        {
            if (HasEnemyFormationChange() || !IsTargetValid())
            {
                if(!TryGetEnemyTarget(out Unit unit)) return;
                EnemyTarget = unit;
            }

            UnitTakeAim();
            CheckSwitchState();
        }
        
        public override bool CheckSwitchState()
        {
            return SwitchToIdleState();
        }
        
        public override void ExitState()
        {
            UnitAnimation.SetFullFireSequenceOff();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private bool SwitchToIdleState()
        {
            bool isTargetDead = EnemyTarget == null || EnemyTarget.IsDead;
            if (isTargetDead && !TryGetEnemyTarget(out Unit _)) LinkedUnitStateMachine.ToDefaultState();
            return isTargetDead;
        }

        private bool HasEnemyFormationChange()
        {
            bool isEnemyFormationChanged = !CacheEnemyFormation.EqualComposition(CurrentEnemyFormation);
            if (isEnemyFormationChanged)
            {
                CacheEnemyFormation = CurrentEnemyFormation;
            }
            return isEnemyFormationChanged;
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
            if (LinkedUnitStateMachine.State != EStates.Fire) return;
            float3 bulletPosition = Position + up() + (float3)UnitTransform.forward;
            ProjectileComponent bullet = ProjectileManager.Instance.UnitRequestAmmo(UnitAttach, bulletPosition);
            bullet.Fire(bulletPosition,AimDirection);
            CurrentRandomAimDirection = randomState.NextFloat2Direction(); // Renew Random Direction
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Shoot Sequence ◈◈◈◈◈◈                                                                          ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void UnitTakeAim()
        {
            if (UnitAnimation.IsPlayingReload) return;
            float3 vectorUnitToTarget = TargetPosition - Position;
            float3 directionUnitToTarget = normalizesafe(vectorUnitToTarget);
            
            //Only on x and y axis (forward(z) axis dont have any value)
            float3 randomDirection = new (CurrentRandomAimDirection * (Accuracy / 10f), 0);
            float3 maxRangePosition = Position + MaxRange * directionUnitToTarget;
            float3 spreadEndPoint = maxRangePosition + randomDirection;
            AimDirection = normalizesafe(spreadEndPoint - Position);
            
            CheckUnitIsFiring();
        }

        private void CheckUnitIsFiring()
        {
            if (UnitAnimation.IsInFiringMode) return;
            UnitAnimation.SetFullFireSequenceOn();
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Get Target Methods ◇◇◇◇◇◇                                                                          │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private bool TryGetEnemyTarget(out Unit target)
        {
            float2 unitPosition = Position.xz;
            using NativeHashSet<int> hullIndices = GetUnitsHullIndices(CacheEnemyFormation);

            (int index, float minDistance) = (-1, INFINITY);
            foreach (int unitIndex in hullIndices)
            {
                float2 enemyPosition = RegimentTargeted.UnitsTransform[unitIndex].position.xz();
                float distance = distancesq(unitPosition, enemyPosition);
                
                if (distance > minDistance) continue;
                (index, minDistance) = (unitIndex, distance);
            }
            
            bool hasTarget = index > -1;
            target = !hasTarget ? null : RegimentTargeted.Units[index];
            return hasTarget;
        }
        
        private NativeHashSet<int> GetUnitsHullIndices(in FormationData enemyFormation)
        {
            int numUnit = enemyFormation.NumUnitsAlive;
            int width = enemyFormation.Width;
            int2 maxWidthDepth = enemyFormation.WidthDepth - 1;
            
            NativeHashSet<int> indices = new (enemyFormation.NumCompleteLine * width, Temp);
            for (int i = 0; i < numUnit; i++)
            {
                int2 coord = GetXY2(i, width);
                bool xyZero = any(coord == zero);
                bool xyMax = any(coord == maxWidthDepth);
                if (!xyZero && !xyMax) continue; 
                indices.Add(i);
            }
            return indices;
        }
    }
}
