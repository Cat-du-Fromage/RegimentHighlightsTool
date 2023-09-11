using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class MeleeAttackOrder : Order
    {
        public Regiment TargetEnemyRegiment { get; private set; }
        public MeleeAttackOrder(Regiment enemyRegiment) : base(EStates.Fire)
        {
            TargetEnemyRegiment = enemyRegiment;
        }
    }
}
