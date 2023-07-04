using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KaizerWald
{
    public sealed class SelectionSystem : HighlightSystem
    {
        public HashSet<Regiment> Regiments => MainSystem.SelectablesRegiments;
        public HighlightRegister PreselectionRegister => Registers[0];
        public HighlightRegister SelectionRegister => Registers[1];
        
        //public SelectionsInfos SelectionsInfo => 
        
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void InitializeController()
        {
            Controller = new RegimentSelectionController(this, Coordinator.HighlightControls, Coordinator.UnitLayerMask);
        }

        protected override void InitializeRegisters()
        {
            GameObject[] prefabs = new[] { Coordinator.PreselectionDefaultPrefab, Coordinator.SelectionDefaultPrefab };
            for (int i = 0; i < prefabs.Length; i++)
            {
                Registers[i] = new HighlightRegister(this, prefabs[i]);
            }
        }

        public override void AddRegiment(Regiment regiment)
        {
            PreselectionRegister.RegisterRegiment(regiment);
            if (regiment.OwnerID != Coordinator.PlayerID) return;
            SelectionRegister.RegisterRegiment(regiment);
        }

        public override void OnShow(Regiment selectableRegiment, int registerIndex)
        {
            selectableRegiment.SetSelectableProperties((ESelection)registerIndex, true);
            base.OnShow(selectableRegiment, registerIndex);
        }
        
        public override void OnHide(Regiment selectableRegiment, int registerIndex)
        {
            selectableRegiment.SetSelectableProperties((ESelection)registerIndex, false);
            base.OnHide(selectableRegiment, registerIndex);
        }

        public override void HideAll(int registerIndex)
        {
            Registers[registerIndex].ActiveHighlights.ForEach(regiment => regiment.SetSelectableProperties((ESelection)registerIndex, false));
            base.HideAll(registerIndex);
        }
    }
}