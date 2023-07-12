using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWald
{
    public class RegimentAbilityManager : MonoBehaviour
    {
        private RegimentManager regimentManager;
        public RegimentAbilityControls Controls { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void Awake()
        {
            regimentManager = GetComponent<RegimentManager>();
            Controls ??= new RegimentAbilityControls();
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Enable | Disable ◈◈◈◈◈◈                                                                        ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void OnEnable()
        {
            Controls.Enable();
            Controls.GeneralAbility.MarchRun.started += ToggleRunMarch;
        }
        
        private void OnDisable()
        {
            Controls.GeneralAbility.MarchRun.started -= ToggleRunMarch;
            Controls.Disable();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void ToggleRunMarch(InputAction.CallbackContext context)
        {
            if (regimentManager.RegimentHighlightSystem.SelectedRegiments.Count == 0) return;
            foreach (Regiment regiment in regimentManager.RegimentHighlightSystem.SelectedRegiments)
            {
                if (!regiment.StateMachine.IsMoving) continue;
                regiment.StateMachine.OnAbilityTrigger(EStates.Move);
            }
        }
    }
}
