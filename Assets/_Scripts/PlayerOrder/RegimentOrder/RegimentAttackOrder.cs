using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class RegimentAttackOrder : RegimentOrder
    {
        public Regiment EnemyTarget { get; private set; }
        public RegimentAttackOrder(Regiment enemyTarget) : base(EStates.Fire)
        {
            EnemyTarget = enemyTarget;
        }
    }
}
