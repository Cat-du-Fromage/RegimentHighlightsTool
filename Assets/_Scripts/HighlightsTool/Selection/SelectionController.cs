using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

using static PlayerControls;
using static UnityEngine.InputSystem.InputAction;

namespace KaizerWald
{
    public sealed class SelectionController : HighlightController
    {
        private SelectionSystem System => (SelectionSystem)HighlightSystem;
        public SelectionActions Actions { get; private set; }

        private InputAction CtrlAction;
        private bool IsCtrlPressed => CtrlAction.IsPressed();

        public SelectionController(HighlightSystem system, SelectionActions actions): base(system, Camera.main)
        {
            Actions = actions;
            OnEnable();

            CtrlAction = Actions.LockSelection;
        }
        
        public override void OnEnable()
        {
            Actions.Enable();
            Actions.LeftMouseClick.canceled += OnLeftMouseReleased;
        }
        
        public override void OnDisable()
        {
            Actions.LeftMouseClick.canceled -= OnLeftMouseReleased;
            Actions.Disable();
        }

        // =============================================================================================================
        // -------- Event Based Controls ----------
        // =============================================================================================================

        public void OnLeftMouseReleased(CallbackContext ctx)
        {
            if (!IsCtrlPressed) DeselectNotPreselected();
            SelectPreselection();
            //HighlightSystem.Register.OnNewSelection();//Experimental SelectionInfos
        }

        private void SelectPreselection()
        {
            List<ISelectableRegiment> preselected = System.PreselectionDependency.PreselectedRegiment;
            foreach (ISelectableRegiment selectable in preselected)
            {
                if (selectable.IsSelected) continue;
                HighlightSystem.OnShow(selectable);
            }
        }

        private void DeselectNotPreselected()
        {
            int iteration = HighlightSystem.Register.ActiveHighlights.Count;
            if (iteration == 0) return;
            for (int i = iteration - 1; i > -1; i--) // we remove element from list, by iterating reverse we stay inbounds
            {
                if (HighlightSystem.Register.ActiveHighlights[i].IsPreselected) continue;
                HighlightSystem.OnHide(HighlightSystem.Register.ActiveHighlights[i]);
            }
        }
    }
}
