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
    public class Unit_RangeAttackState : UnitStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        //ATTENTION PAS { get; private set; } sinon NextFloat2Direction ne bougera pas
        private Random randomState;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Unit UnitEnemyTarget { get; private set; }
        public float2 CurrentRandomAimDirection { get; private set; }
        public float3 AimDirection { get; private set; }
        public FormationData CacheEnemyFormation { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                   ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private Regiment RegimentEnemyTarget => RegimentBlackboard.EnemyTarget;
        public float3 TargetPosition => RegimentEnemyTarget.transform.position;
        private FormationData CurrentEnemyFormation => RegimentEnemyTarget.CurrentFormation;
        private int MaxRange => UnitAttach.RegimentAttach.RegimentType.Range;
        private int Accuracy => UnitAttach.RegimentAttach.RegimentType.Accuracy;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Unit_RangeAttackState(UnitBehaviourTree behaviourTree) : base(behaviourTree, EStates.Fire)
        {
            randomState = Random.CreateFromIndex((uint)(abs(UnitAttach.GetInstanceID()) + UnitAttach.IndexInRegiment));
            CurrentRandomAimDirection = randomState.NextFloat2Direction();
            UnitAnimation.OnShootEvent += OnFireEvent;
        }

        public override bool ConditionEnter()
        {
            bool regimentIsFiring = BehaviourTree.RegimentBehaviourTree.IsFiring;
            bool isFirstLine = IndexInRegiment < UnitAttach.RegimentAttach.CurrentFormation.Width;
            return regimentIsFiring && isFirstLine;
        }

        public override void OnSetup(Order order)
        {
            //TryGetEnemyTarget(out Unit unit);
            //UnitEnemyTarget = unit;
        }

        public override void OnEnter()
        {
            TryGetEnemyTarget(out Unit unit);
            UnitEnemyTarget = unit;
            UnitAnimation.SetFullFireSequenceOn();
        }

        public override void OnUpdate()
        {
            Retarget();
            UnitTakeAim();
        }

        public override void OnExit()
        {
            UnitAnimation.SetFullFireSequenceOff();
        }

        public override EStates ShouldExit()
        {
            // IL FAUT FORCER LE REARRANGEMENT
            //Ajouter Melee
            return StateIdentity == RegimentState ? StateIdentity : RegimentState;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Retarget()
        {
            if ((!HasEnemyFormationChange() && IsTargetValid()) || !TryGetEnemyTarget(out Unit unit)) return;
            //if(!TryGetEnemyTarget(out Unit unit)) return;
            UnitEnemyTarget = unit;
        }

        private bool HasEnemyFormationChange()
        {
            bool isEnemyFormationChanged = !CacheEnemyFormation.EqualComposition(CurrentEnemyFormation);
            if (isEnemyFormationChanged) CacheEnemyFormation = CurrentEnemyFormation;
            return isEnemyFormationChanged;
        }
        
        private bool IsTargetValid()
        {
            return UnitEnemyTarget != null && !UnitEnemyTarget.IsDead;
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Animation Event ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        // Call : objectAttach.Animation.OnShootEvent
        private void OnFireEvent(AnimationEvent animationEvent)
        {
            //float3 position = UnitTransform.position;
            Vector3 bulletPosition = UnitTransform.position + Vector3.up + UnitTransform.forward;
            ProjectileComponent bullet = ProjectileManager.Instance.UnitRequestAmmo(UnitAttach, bulletPosition);
            bullet.Fire(bulletPosition,AimDirection);
            CurrentRandomAimDirection = randomState.NextFloat2Direction(); // Renew Random Direction
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Shoot Sequence ◈◈◈◈◈◈                                                                              ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void UnitTakeAim()
        {
            if (UnitAnimation.IsPlayingReload) return;
            float3 position = UnitTransform.position;
            float3 vectorUnitToTarget = TargetPosition - position;
            float3 directionUnitToTarget = normalizesafe(vectorUnitToTarget);
            
            //Only on x and y axis (forward(z) axis dont have any value)
            float3 randomDirection = new (CurrentRandomAimDirection * (Accuracy / 10f), 0);
            float3 maxRangePosition = position + MaxRange * directionUnitToTarget;
            float3 spreadEndPoint = maxRangePosition + randomDirection;
            AimDirection = normalizesafe(spreadEndPoint - position);
            
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
            float2 unitPosition = UnitTransform.position.xz();
            using NativeHashSet<int> hullIndices = GetUnitsHullIndices(RegimentBlackboard.EnemyFormation);

            (int index, float minDistance) = (-1, INFINITY);
            foreach (int unitIndex in hullIndices)
            {
                float2 enemyPosition = RegimentBlackboard.EnemyTarget.UnitsTransform[unitIndex].position.xz();
                float distance = distancesq(unitPosition, enemyPosition);
                
                if (distance > minDistance) continue;
                (index, minDistance) = (unitIndex, distance);
            }
            
            bool hasTarget = index > -1;
            target = !hasTarget ? null : RegimentBlackboard.EnemyTarget.Units[index];
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
