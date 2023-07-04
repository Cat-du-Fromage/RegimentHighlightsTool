﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KaizerWald
{
    public sealed class SelectionSystem : HighlightSystem
    {
        public static readonly int PreselectionRegisterIndex = 0;
        public static readonly int SelectionRegisterIndex = 1;
        
        public HashSet<Regiment> Regiments => MainSystem.AllRegiments;
        public HighlightRegister PreselectionRegister => Registers[PreselectionRegisterIndex];
        public HighlightRegister SelectionRegister => Registers[SelectionRegisterIndex];
        
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

        public override void OnShow(Regiment regiment, int registerIndex)
        {
            ((ISelectable)regiment).SetSelectableProperty(registerIndex, true);
            base.OnShow(regiment, registerIndex);
        }
        
        public override void OnHide(Regiment regiment, int registerIndex)
        {
            ((ISelectable)regiment).SetSelectableProperty(registerIndex, false);
            base.OnHide(regiment, registerIndex);
        }

        public override void HideAll(int registerIndex)
        {
            Registers[registerIndex].ActiveHighlights.ForEach(regiment => ((ISelectable)regiment).SetSelectableProperty(registerIndex, false));
            base.HideAll(registerIndex);
        }
    }
}