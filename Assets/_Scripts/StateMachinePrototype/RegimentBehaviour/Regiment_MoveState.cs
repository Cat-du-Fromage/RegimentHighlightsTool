using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace KaizerWald
{
    public class Regiment_MoveState : RegimentStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public float3 Destination { get; private set; }
        public FormationData FormationDestination { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment_MoveState(RegimentBehaviourTree behaviourTree, Blackboard blackBoard) : base(behaviourTree,blackBoard,EStates.Move)
        {
        }

        public override void OnSetup(Order order)
        {
            //
        }

        public override void OnEnter()
        {
            //
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
    }
}
