using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class Unit : MonoBehaviour
    {
        [field: SerializeField] public Regiment RegimentAttach { get; private set; }
        [field: SerializeField] public int IndexInRegiment { get; private set; }

        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Unity Events ◇◇◇◇◇                                                                                   ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void OnDestroy()
        {
            OnDeath();
        }

        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Initialization Methods ◇◇◇◇◇                                                                         ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Unit InitializeUnit(Regiment regiment, int indexInRegiment, int unitLayerIndex)
        {
            RegimentAttach = regiment;
            IndexInRegiment = indexInRegiment;
            gameObject.layer = unitLayerIndex;
            return this;
        }
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Class Methods ◇◇◇◇◇                                                                                  ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void OnDeath()
        {
            if (RegimentAttach == null) return;
            RegimentAttach.OnDeadUnit(this);
        }
    }
}