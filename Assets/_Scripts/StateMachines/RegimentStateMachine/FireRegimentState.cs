using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static KaizerWald.KzwMath;
using static Unity.Mathematics.math;
using static Unity.Mathematics.int2;

using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

namespace KaizerWald
{
    public class FireRegimentState : RegimentState
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment RegimentTargeted { get; private set; }
        public FormationData CacheEnemyFormation { get; private set; }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private Formation CurrentEnemyFormation => RegimentTargeted.CurrentFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public FireRegimentState(RegimentStateMachine regimentStateMachine) : base(regimentStateMachine, EStates.Fire)
        {
            
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override void SetupState(Order order)
        {
            AttackOrder attackOrder = (AttackOrder)order;
            RegimentTargeted = attackOrder.TargetEnemyRegiment;
            CacheEnemyFormation = attackOrder.TargetEnemyRegiment.CurrentFormation;
        }
        
        public override void EnterState()
        {
            
        }

        public override void UpdateState()
        {
            //Check EnemyFormation: changes detected? how ?
            if (HasEnemyFormationChange() && CurrentEnemyFormation.NumUnitsAlive != 0)
            {
                CacheEnemyFormation = CurrentEnemyFormation;
            }
            CheckSwitchState();
        }
        
        public override bool CheckSwitchState()
        {
            return SwitchToIdleState(); // Is Regiment Still Alive
        }

        public override void ExitState()
        {
            
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private bool HasEnemyFormationChange()
        {
            bool3 hasEnemyFormationChange = new bool3
            (
                CacheEnemyFormation.NumUnitsAlive != CurrentEnemyFormation.NumUnitsAlive,
                CacheEnemyFormation.WidthDepth != CurrentEnemyFormation.WidthDepth
            );
            return any(hasEnemyFormationChange);
        }

        private bool SwitchToIdleState()
        {
            bool isTargetDead = RegimentTargeted == null || RegimentTargeted.Units.Count == 0;
            if (isTargetDead) LinkedRegimentStateMachine.ToDefaultState();
            return isTargetDead;
        }
    }
}
