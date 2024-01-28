using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Kaizerwald.HighlightRegimentManager;

namespace Kaizerwald
{
    public class Blackboard
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private readonly Regiment RegimentReference;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment EnemyTarget { get; private set; }
        public bool HasTarget { get; private set; }
        public bool IsChasing { get; private set; }
        public int TargetedRegimentID { get; private set; }
        public float3 Destination { get; private set; }
        
        public FormationData DestinationFormation { get; private set; }
        public FormationData CacheEnemyFormation { get; private set; }
        
        //ATTACK
        public HashSet<Regiment> Aggressors { get; private set; } = new (10);
        public HashSet<Unit> UnitsInMelee { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public RegimentType RegimentStats => RegimentReference.RegimentType;
        public FormationData CurrentFormation => RegimentReference.CurrentFormation;
        public FormationData EnemyFormation => EnemyTarget.CurrentFormation;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Setters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        public void SetDestination(float3 value) => Destination = value;
        public void SetDestinationFormation(FormationData destinationFormation) => DestinationFormation = destinationFormation;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Blackboard(Regiment regiment)
        {
            RegimentReference = regiment;
            Destination = regiment.RegimentPosition;
            DestinationFormation = regiment.CurrentFormation;
            HasTarget = false;
            IsChasing = false;
            TargetedRegimentID = -1;
            UnitsInMelee = new HashSet<Unit>(regiment.Units.Count);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void UpdateInformation()
        {
            UpdateEnemyInfos();
            UpdateDestinationInfos();
        }

        private void UpdateEnemyInfos()
        {
            if (!HasTarget || EnemyTarget != null) return;
            ResetTarget();
        }
        
        private void UpdateDestinationInfos()
        {
            if (CurrentFormation.NumUnitsAlive == DestinationFormation.NumUnitsAlive) return;
            DestinationFormation = new FormationData(CurrentFormation, CurrentFormation.NumUnitsAlive);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Order ◈◈◈◈◈◈                                                                                            ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Order OnOrder(Order order)
        {
            return order switch
            {
                MoveOrder moveOrder        => OnMoveOrder(moveOrder),
                RangeAttackOrder fireOrder => OnRangeAttackOrder(fireOrder),
                _ => order
            };
        }

        private Order OnMoveOrder(MoveOrder moveOrder)
        {
            Destination = moveOrder.LeaderDestination;
            DestinationFormation = moveOrder.FormationDestination;
            ResetTarget();
            return moveOrder;
        }

        private Order OnRangeAttackOrder(RangeAttackOrder rangeAttackOrder)
        {
            SetChaseEnemyTarget(rangeAttackOrder.TargetEnemyRegiment);
            bool isTargetInRange = StateExtension.IsTargetRegimentInRange(RegimentReference, EnemyTarget, RegimentStats.Range);
            if (!isTargetInRange)
            {
                SetChaseDestination(CurrentFormation);
                //UPDATE PLACEMENT HERE??
                //RegimentManager.Instance.UpdatePlacements(RegimentReference);
                MoveOrder moveOrder = new MoveOrder(CurrentFormation, Destination);
                return moveOrder;
            }
            return rangeAttackOrder;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Target Attribution ◈◈◈◈◈◈                                                                               ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        
        public bool SetEnemyTarget(int targetId)
        {
            return HighlightRegimentManager.Instance.RegimentExist(targetId) ? SetTarget(RegimentManager.Instance.RegimentsByID[targetId]) : ResetTarget();
        }
        
        //we still check regiment is registered after checking it is not null
        public bool SetEnemyTarget(Regiment regimentTarget)
        {
            return regimentTarget != null ? SetEnemyTarget(regimentTarget.RegimentID) : ResetTarget();
        }
            
        public bool SetChaseEnemyTarget(int targetId, FormationData formationDestination)
        {
            IsChasing = SetEnemyTarget(targetId);
            SetChaseDestination(formationDestination);
            return IsChasing;
        }

        public bool SetChaseEnemyTarget(Regiment regimentTarget, FormationData formationDestination)
        {
            IsChasing = SetEnemyTarget(regimentTarget);
            SetChaseDestination(formationDestination);
            return IsChasing;
        }
            
        public bool SetChaseEnemyTarget(int targetId) => SetChaseEnemyTarget(targetId, CurrentFormation);
        public bool SetChaseEnemyTarget(Regiment regimentTarget) => SetChaseEnemyTarget(regimentTarget, CurrentFormation);
    
        public bool ResetTarget()
        {
            IsChasing = false; //we want to reset it by default
            HasTarget = false;
            EnemyTarget = null;
            TargetedRegimentID = -1;
            return HasTarget;
        }
        
        private bool SetTarget(Regiment regimentTarget)
        {
            IsChasing = false; //we want to reset it by default
            HasTarget = true;
            EnemyTarget = regimentTarget;
            TargetedRegimentID = regimentTarget.RegimentID;
            CacheEnemyFormation = regimentTarget.CurrentFormation;
            return HasTarget;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Check Enemies at Range ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void SetChaseDestination(FormationData formationDestination)
        {
            if (!HasTarget) return;
            //float2 enemyCenterFormation = GetCenterEnemyFormation();
            float2 enemyCenterFormation = EnemyTarget.RegimentPosition.xz;
            float2 directionEnemyToRegiment = enemyCenterFormation.DirectionTo(RegimentReference.RegimentPosition.xz);

            //FAUX : il faut prendre les 4 coins (voire + les milieux de chaque côtés) de la formation
            //ET trouver le points le plus proche ET ensite calculer la destination
            int radiusAroundTarget = RegimentStats.Range;

            float2 destinationCalculated = enemyCenterFormation + directionEnemyToRegiment * radiusAroundTarget;
            //TODO: Determiner la valeur de Y
            Destination = new float3(destinationCalculated.x, 0, destinationCalculated.y);
            DestinationFormation = formationDestination;
        }

        private float2 GetCenterEnemyFormation()
        {
            float midDepthEnemyFormation = EnemyTarget.CurrentFormation.Depth / 2f;
            float2 enemyForward = EnemyTarget.RegimentTransform.forward.xz();
            float2 enemyCenterFormation = EnemyTarget.RegimentPosition.xz - enemyForward * midDepthEnemyFormation;
            return enemyCenterFormation;
        }
    }
}
