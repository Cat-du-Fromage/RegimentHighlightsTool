using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace KaizerWald
{
    public abstract class RegimentStateBase : StateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Blackboard RegimentBlackboard { get; protected set; }
        public RegimentBehaviourTree BehaviourTree { get; private set; }
        public Regiment RegimentAttach { get; protected set; }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public float3 Position => BehaviourTree.Position;
        public float3 Forward  => BehaviourTree.Forward;
        public float3 Back     => BehaviourTree.Back;
        public float3 Right    => BehaviourTree.Right;
        public float3 Left     => BehaviourTree.Left;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Setter ◈◈◈◈◈◈                                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected RegimentStateBase(RegimentBehaviourTree behaviourTree, Blackboard blackboard, EStates stateIdentity) : base(stateIdentity)
        {
            RegimentBlackboard = blackboard;
            BehaviourTree = behaviourTree;
            RegimentAttach = behaviourTree.RegimentAttach;
        }
    }
}
