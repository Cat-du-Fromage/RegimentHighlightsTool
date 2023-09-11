using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        protected Transform UnitTransform => BehaviourTree.CachedTransform;
        protected UnitAnimation UnitAnimation => UnitAttach.Animation;
        protected int IndexInRegiment => UnitAttach.IndexInRegiment;
        protected EStates RegimentState => BehaviourTree.RegimentState;
        
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

        protected EStates GetRegimentState()
        {
            return BehaviourTree.RegimentState;
        }
    }
}
