using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Kaizerwald
{
    public abstract class RegimentStateBase : StateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public RegimentBehaviourTree BehaviourTree { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Blackboard RegimentBlackboard => BehaviourTree.RegimentBlackboard;
        public Regiment RegimentAttach => BehaviourTree.RegimentAttach;
    
        public float3 Position => BehaviourTree.Position;
        public float3 Forward  => BehaviourTree.Forward;
        public float3 Back     => BehaviourTree.Back;
        public float3 Right    => BehaviourTree.Right;
        public float3 Left     => BehaviourTree.Left;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Setter ◈◈◈◈◈◈                                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected RegimentStateBase(RegimentBehaviourTree behaviourTree, EStates stateIdentity) : base(stateIdentity)
        {
            BehaviourTree = behaviourTree;
            //RegimentAttach = behaviourTree.RegimentAttach;
        }
    }
}
