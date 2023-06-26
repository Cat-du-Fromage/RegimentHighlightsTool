using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    /// <summary>
    /// REFACTOR: Make it monobehaviour component
    /// </summary>
    public sealed class SelectionSystem : HighlightSystem
    {
        public RegimentHighlightSystem MainSystem { get; private set; }
        public List<SelectableRegiment> Regiments => MainSystem.Coordinator.SelectableRegiments;
        public HighlightRegister PreselectionRegister => Registers[0];
        public HighlightRegister SelectionRegister => Registers[1];
        
        private void Awake()
        {
            MainSystem = GetComponent<RegimentHighlightSystem>();
            HighlightCoordinator coordinator = MainSystem.Coordinator;
            Controller = new RegimentSelectionController(this, coordinator.HighlightControls, coordinator.UnitLayerMask);
            InitializeRegisters(coordinator);
        }

        private void InitializeRegisters(HighlightCoordinator coordinator)
        {
            GameObject[] prefabs = new[] { coordinator.PreselectionDefaultPrefab, coordinator.SelectionDefaultPrefab };
            for (int i = 0; i < Registers.Length; i++)
            {
                Registers[i] = new HighlightRegister(this, prefabs[i]);
            }
        }
        /*
        #region Constructor
        public SelectionSystem(RegimentHighlightSystem mainSystem, GameObject[] prefabs, PlayerControls controls, LayerMask unitLayerMask)
        {
            MainSystem = mainSystem;
            Controller = new RegimentSelectionController(this, controls, unitLayerMask);
            for (int i = 0; i < Registers.Length; i++)
            {
                Registers[i] = new HighlightRegister(this, prefabs[i]);
            }
        }

        public SelectionSystem(RegimentHighlightSystem mainSystem, GameObject[] prefabs, PlayerControls controls, LayerMask unitLayerMask, List<SelectableRegiment> regiments) 
            : this(mainSystem, prefabs, controls, unitLayerMask)
        {
            regiments.ForEach(AddRegiment);
        }
        #endregion
        */
        public override void AddRegiment(SelectableRegiment regiment)
        {
            Array.ForEach(Registers, register => register.RegisterRegiment(regiment));
        }

        public override void RemoveRegiment(SelectableRegiment regiment)
        {
            Array.ForEach(Registers, register => register.UnregisterRegiment(regiment));
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