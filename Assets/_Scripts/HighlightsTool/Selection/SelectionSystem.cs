using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KaizerWald
{
    public sealed class SelectionSystem : HighlightSystem
    {
        public PreselectionSystem PreselectionDependency { get; private set; }
        public SelectionSystem(HighlightCoordinator coordinator, GameObject defaultPrefab, PlayerControls controls, PreselectionSystem preselectionSystem) : base(coordinator, defaultPrefab)
        {
            Coordinator = coordinator;
            Register = new SelectionRegister(this, defaultPrefab);
            Controller = new SelectionController(this, controls.Selection);
            PreselectionDependency = preselectionSystem;

            
            //PreselectionDependency.OnControllerEvent += ((SelectionController)Controller).OnLeftMouseReleased;
        }
        
        public override void OnShow(ISelectableRegiment selectableRegiment)
        {
            if (Register.ActiveHighlights.Contains(selectableRegiment)) return; // already Enable
            base.OnShow(selectableRegiment);
            selectableRegiment.IsSelected = true;
            Register.ActiveHighlights.Add(selectableRegiment);
        }

        public override void OnHide(ISelectableRegiment selectableRegiment)
        {
            base.OnHide(selectableRegiment);
            selectableRegiment.IsSelected = false;
            Register.ActiveHighlights.Remove(selectableRegiment);
        }
        
        public override void HideAll()
        {
            base.HideAll();
            Register.ActiveHighlights.ForEach(regiment => { regiment.IsSelected = false; });
            Register.ActiveHighlights.Clear();
        }
    }
}
