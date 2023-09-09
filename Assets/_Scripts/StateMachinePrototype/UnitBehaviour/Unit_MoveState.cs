using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace KaizerWald
{
    public class Unit_MoveState : UnitStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private Dictionary<Regiment, float> EngagementInvulnerability = new (4);

        private float MoveSpeed = 1f;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public float3 LeaderDestination { get; private set; }
        public FormationData FormationDestination { get; private set; }
        public float3 UnitDestination { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Unit_MoveState(UnitBehaviourTree behaviourTree) : base(behaviourTree, EStates.Move)
        {
            Sequences = new List<EStates>()
            {
                EStates.Move,
                EStates.Idle
            };
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public override void OnSetup(Order order)
        {
            MoveOrder moveOrder = (MoveOrder)order;
            FormationDestination = moveOrder.FormationDestination;
            LeaderDestination = moveOrder.LeaderDestination;
            UnitDestination = FormationDestination.GetUnitRelativePositionToRegiment3D(IndexInRegiment, LeaderDestination);
        }

        public override void OnEnter()
        {
            UnitAnimation.SetMarching();
        }

        public override void OnUpdate()
        {
            //
        }

        public override void OnExit()
        {
            //
        }

        public override EStates ShouldExit()
        {
            return StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        private void MoveUnit()
        {
            float3 direction = normalizesafe(UnitDestination - BehaviourTree.Position);
            float3 translation = Time.deltaTime * MoveSpeed * direction;
            UnitTransform.Translate(translation, Space.World);
            UnitTransform.LookAt(BehaviourTree.Position + FormationDestination.Direction3DForward);
        }
    }
}
