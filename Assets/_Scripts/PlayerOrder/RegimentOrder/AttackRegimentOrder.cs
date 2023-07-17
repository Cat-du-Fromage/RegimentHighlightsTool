using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class AttackRegimentOrder : RegimentOrder
    {
        public Regiment EnemyTarget { get; private set; }
        public AttackRegimentOrder(Regiment receiver, EStates state, Regiment enemyTarget) : base(receiver, state)
        {
            EnemyTarget = enemyTarget;
        }
    }
}
