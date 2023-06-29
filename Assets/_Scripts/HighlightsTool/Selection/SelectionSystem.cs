using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KaizerWald
{
    public sealed class SelectionSystem : HighlightSystem
    {
        public RegimentHighlightSystem MainSystem { get; private set; }
        public HashSet<SelectableRegiment> Regiments => MainSystem.SelectablesRegiments;
        public HighlightRegister PreselectionRegister => Registers[0];
        public HighlightRegister SelectionRegister => Registers[1];
        
        private void Awake()
        {
            MainSystem = GetComponent<RegimentHighlightSystem>();
            HighlightCoordinator coordinator = FindFirstObjectByType<HighlightCoordinator>();
            Controller = new RegimentSelectionController(this, coordinator.HighlightControls, coordinator.UnitLayerMask);
            InitializeRegisters(coordinator);
        }

        private void InitializeRegisters(HighlightCoordinator coordinator)
        {
            List<GameObject> prefabs = new() { coordinator.PreselectionDefaultPrefab, coordinator.SelectionDefaultPrefab };
            //prefabs.ForEach(prefab => Registers.Append(new HighlightRegister(this, prefab)));
            for (int i = 0; i < Registers.Length; i++)
            {
                Registers[i] = new HighlightRegister(this, prefabs[i]);
            }
        }
        
        public override void OnShow(SelectableRegiment selectableRegiment, int registerIndex)
        {
            base.OnShow(selectableRegiment, registerIndex);
            selectableRegiment.SetSelectableProperties((ESelection)registerIndex, true);
            Registers[registerIndex].ActiveHighlights.Add(selectableRegiment);
        }
        
        public override void OnHide(SelectableRegiment selectableRegiment, int registerIndex)
        {
            base.OnHide(selectableRegiment, registerIndex);
            selectableRegiment.SetSelectableProperties((ESelection)registerIndex, false);
            Registers[registerIndex].ActiveHighlights.Remove(selectableRegiment);
        }

        public override void HideAll(int registerIndex)
        {
            base.HideAll(registerIndex);
            Registers[registerIndex].ActiveHighlights.ForEach(regiment => regiment.SetSelectableProperties((ESelection)registerIndex, false));
            Registers[registerIndex].ActiveHighlights.Clear();
        }
    }
}