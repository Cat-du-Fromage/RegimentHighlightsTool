using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using static KaizerWald.RegimentManager;

namespace KaizerWald
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
        
        //public FormationData CurrentFormation { get; private set; }
        public FormationData DestinationFormation { get; private set; }
        public FormationData CacheEnemyFormation { get; private set; }
        
        //ATTACK
        public HashSet<Regiment> Aggressors { get; private set; } = new (10);
        public HashSet<Unit> UnitsInMelee { get; private set; }
        
        
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public FormationData CurrentFormation => RegimentReference.CurrentFormation;
        public FormationData EnemyFormation => EnemyTarget.CurrentFormation;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Setter ◈◈◈◈◈◈                                                                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void SetIsChasing(bool value) => IsChasing = value;
        public void SetTargetedRegimentID(int value) => TargetedRegimentID = value;
        
        public bool SetEnemyTarget(int targetId)
        {
            IsChasing = false;
            bool isTargetValid = targetId != -1;
            if (isTargetValid)
            {
                HasTarget = Instance.RegimentsByID.ContainsKey(targetId);
                if(HasTarget)
                {
                    TargetedRegimentID = targetId;
                    EnemyTarget = Instance.RegimentsByID[targetId];
                    CacheEnemyFormation = EnemyTarget.CurrentFormation;
                    return true;
                }
            }
            ResetTarget();
            return false;
        }
        
        public bool SetEnemyTarget(Regiment regimentTarget)
        {
            IsChasing = false;
            TargetedRegimentID = regimentTarget.RegimentID;
            EnemyTarget = regimentTarget;
            CacheEnemyFormation = regimentTarget.CurrentFormation;
            if (Instance.RegimentsByID.ContainsKey(TargetedRegimentID)) return true;
            ResetTarget();
            return false;
        }
        
        public bool SetEnemyChase(int targetId, FormationData formationDestination)
        {
            IsChasing = SetEnemyTarget(targetId);
            SetChaseDestination(formationDestination);
            return IsChasing;
        }

        public bool SetEnemyChase(Regiment regimentTarget, FormationData formationDestination)
        {
            IsChasing = SetEnemyTarget(regimentTarget);
            SetChaseDestination(formationDestination);
            return IsChasing;
        }

        public void SetDestination(float3 value) => Destination = value;
        public void SetDestinationFormation(FormationData destinationFormation) => DestinationFormation = destinationFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Blackboard(Regiment regiment)
        {
            RegimentReference = regiment;
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
        }

        private void UpdateEnemyInfos()
        {
            if (!HasTarget || EnemyTarget != null) return;
            ResetTarget();
        }

        public void ResetTarget()
        {
            TargetedRegimentID = -1;
            HasTarget = false;
            IsChasing = false;
            EnemyTarget = null;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Check Enemies at Range ◈◈◈◈◈◈                                                                      ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void SetChaseDestination(FormationData formationDestination)
        {
            if (!HasTarget) return;
            float2 enemyCenterFormation = GetCenterEnemyFormation();
            float2 directionEnemyToRegiment = enemyCenterFormation.DirectionTo(RegimentReference.RegimentPosition.xz);

            int radiusAroundTarget = RegimentReference.RegimentType.Range;

            float2 destinationCalculated = enemyCenterFormation + directionEnemyToRegiment * radiusAroundTarget;
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
