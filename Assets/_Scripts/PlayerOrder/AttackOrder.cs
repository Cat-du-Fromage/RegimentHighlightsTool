using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class AttackOrder : Order
    {
        public Regiment TargetEnemyRegiment { get; private set; }
        public AttackOrder(Regiment enemyRegiment) : base(EStates.Fire)
        {
            TargetEnemyRegiment = enemyRegiment;
        }
    }
}
