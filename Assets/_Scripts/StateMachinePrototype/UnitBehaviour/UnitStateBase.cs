using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace KaizerWald
{
    public abstract class UnitStateBase : StateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public UnitBehaviourTree BehaviourTree { get; protected set; }
        public Unit UnitAttach { get; protected set; }
        public Blackboard RegimentBlackboard { get; protected set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                   ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        protected Regiment ParentRegiment => BehaviourTree.UnitAttach.RegimentAttach;
        protected Transform UnitTransform => BehaviourTree.CachedTransform;
        protected UnitAnimation UnitAnimation => UnitAttach.Animation;
        protected int IndexInRegiment => UnitAttach.IndexInRegiment;
        protected EStates RegimentState => BehaviourTree.RegimentState;
        
        
        protected float3 Position => BehaviourTree.Position;
        protected float3 Forward  => BehaviourTree.Forward;
        protected float3 Back     => BehaviourTree.Back;
        protected float3 Right    => BehaviourTree.Right;
        protected float3 Left     => BehaviourTree.Left;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected UnitStateBase(UnitBehaviourTree behaviourTree, EStates stateIdentity) : base(stateIdentity)
        {
            BehaviourTree = behaviourTree;
            UnitAttach = behaviourTree.UnitAttach;
            RegimentBlackboard = behaviourTree.RegimentBehaviourTree.RegimentBlackboard;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    }
}
