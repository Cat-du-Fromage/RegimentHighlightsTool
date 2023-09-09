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
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        protected Transform UnitTransform => BehaviourTree.CachedTransform;
        protected UnitAnimation UnitAnimation => UnitAttach.Animation;
        protected int IndexInRegiment => UnitAttach.IndexInRegiment;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected UnitStateBase(UnitBehaviourTree behaviourTree, EStates stateIdentity) : base(stateIdentity)
        {
            BehaviourTree = behaviourTree;
            UnitAttach = behaviourTree.UnitAttach;
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
