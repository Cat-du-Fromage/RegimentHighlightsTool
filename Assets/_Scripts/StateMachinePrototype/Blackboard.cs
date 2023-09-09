using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static KaizerWald.RegimentManager;

namespace KaizerWald
{
    public class Blackboard
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private bool hasTarget;
        private bool isChasing;
        private int targetedRegimentID;
        private Regiment enemyTarget;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public bool HasTarget => hasTarget;
        public bool IsChasing => isChasing;
        public int TargetedRegimentID => targetedRegimentID;
        public Regiment EnemyTarget => enemyTarget;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Setter ◈◈◈◈◈◈                                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void SetHasTarget(bool value) => hasTarget = value;
        public void SetIsChasing(bool value) => isChasing = value;
        public void SetTargetedRegimentID(int value) => targetedRegimentID = value;
        
        public bool SetEnemyTarget(int targetId)
        {
            bool isTargetValid = targetId != -1;
            if (!isTargetValid)
            {
                ResetTarget();
                return false;
            }
            else
            {
                targetedRegimentID = targetId;
                return Instance.RegimentsByID.TryGetValue(targetedRegimentID, out enemyTarget);
            }
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Blackboard()
        {
            hasTarget = false;
            isChasing = false;
            targetedRegimentID = -1;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public void ResetTarget()
        {
            targetedRegimentID = -1;
            enemyTarget = null;
        }
    }
}
