using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class PreselectionSystem : HighlightSystem
    {
        public List<ISelectableRegiment> PreselectedRegiment => Register.ActiveHighlights;
        public PreselectionSystem(HighlightCoordinator coordinator, GameObject defaultPrefab, LayerMask playerUnitLayer, PlayerControls controls) : base(coordinator, defaultPrefab)
        {
            Register = new PreselectionRegister(this, defaultPrefab);
            Controller = new PreselectionController(this, playerUnitLayer, controls);
        }

        public override void Notify()
        {
            base.Notify();
            Coordinator.OnNotification(this);
        }

        public override void OnShow(ISelectableRegiment selectableRegiment)
        {
            if (Register.ActiveHighlights.Contains(selectableRegiment)) return; // already Enable
            base.OnShow(selectableRegiment);
            selectableRegiment.IsPreselected = true;
            Register.ActiveHighlights.Add(selectableRegiment);
        }

        public override void OnHide(ISelectableRegiment selectableRegiment)
        {
            base.OnHide(selectableRegiment);
            selectableRegiment.IsPreselected = false;
            Register.ActiveHighlights.Remove(selectableRegiment);
        }
        
        public override void HideAll()
        {
            foreach (ISelectableRegiment activeHighlight in Register.ActiveHighlights)
            {
                activeHighlight.IsPreselected = false;
                foreach (HighlightBehaviour highlight in Register.Records[activeHighlight.RegimentID])
                {
                    if (highlight == null) continue;
                    highlight.Hide();
                }
            }
            Register.ActiveHighlights.Clear();
        }
    }
}
