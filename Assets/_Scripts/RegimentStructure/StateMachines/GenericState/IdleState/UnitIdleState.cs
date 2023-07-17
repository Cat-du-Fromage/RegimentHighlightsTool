using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    /// <summary>
    /// Idle Means: Stop Moving, Stop Firing(can be forced)
    /// During Idle: will rearrange, respond to melee attack
    /// </summary>
    public class UnitIdleState : IdleState<Unit>
    {
        private readonly UnitAnimation unitAnimation;
        
        public UnitIdleState(Unit unit) : base(unit)
        {
            if (!unit.TryGetComponent(out unitAnimation))
            {
                Debug.LogError($"Unit: {unit.name} don't have 'UnitAnimation' Component");
            }
        }

        public override void OnStateEnter(Order<Unit> order)
        {
            unitAnimation.SetIdle();
            return;
        }

        public override void OnStateUpdate()
        {
            return;
        }

        public override bool OnTransitionCheck()
        {
            // May be in combat! but not in the first Row
            // => return (ObjAttach.IndexInRegiment < Regiment.Formation.Width)
            
            //Check if enemies in Range (if autoFire is Active)!! maybe regiment shall be in charge of this?
            return false;
        }

        public override void OnStateExit()
        {
            return;
        }
    }
}