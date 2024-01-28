using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kaizerwald
{
    public sealed class SelectionSystem : HighlightSystem
    {
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public static readonly int PreselectionRegisterIndex = 0;
        public static readonly int SelectionRegisterIndex = 1;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                    ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public List<HighlightRegiment> Regiments => Coordinator.RegimentsByPlayerID[Coordinator.PlayerID];
        public HighlightRegister PreselectionRegister => Registers[PreselectionRegisterIndex];
        public HighlightRegister SelectionRegister => Registers[SelectionRegisterIndex];
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public SelectionSystem(HighlightRegimentManager manager) : base(manager)
        {
            InitializeController();
            InitializeRegisters();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                       ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
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
            //prefabs.ForEachWithIndex(index => Registers[index] = new HighlightRegister(this, prefabs[index]));
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Behaviours Methods ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ NEW HIGHLIGHT REGIMENT ◈◈◈◈◈◈                                                                       ║
        public override void AddRegiment(HighlightRegiment regiment, List<GameObject> units)
        {
            PreselectionRegister.RegisterRegiment(regiment, units);
            if (regiment.OwnerID != Coordinator.PlayerID) return;
            SelectionRegister.RegisterRegiment(regiment, units);
        }
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        /*
        public override void AddRegiment(Regiment regiment)
        {
            PreselectionRegister.RegisterRegiment(regiment);
            if (regiment.OwnerID != Coordinator.PlayerID) return;
            SelectionRegister.RegisterRegiment(regiment);
        }
        */
        public override void OnShow(HighlightRegiment regiment, int registerIndex)
        {
            ((ISelectable)regiment).SetSelectableProperty(registerIndex, true);
            base.OnShow(regiment, registerIndex);
        }
        
        public override void OnHide(HighlightRegiment regiment, int registerIndex)
        {
            ((ISelectable)regiment).SetSelectableProperty(registerIndex, false);
            base.OnHide(regiment, registerIndex);
        }

        public override void HideAll(int registerIndex)
        {
            foreach (HighlightRegiment regiment in Registers[registerIndex].ActiveHighlights)
            {
                ((ISelectable)regiment).SetSelectableProperty(registerIndex, false);
            }
            base.HideAll(registerIndex);
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Rearrangement ◈◈◈◈◈◈                                                                                ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void ResizeAndReformRegister(int registerIndex, HighlightRegiment regiment, int numHighlightToKeep)
        {
            if (!Registers[registerIndex].Records.ContainsKey(regiment.RegimentID)) return;
            HighlightBehaviour[] newRecordArray = Registers[registerIndex][regiment.RegimentID].Slice(0, numHighlightToKeep);
            for (int i = 0; i < numHighlightToKeep; i++)
            {
                HighlightBehaviour highlight = newRecordArray[i];
                HighlightUnit unitToAttach = regiment.HighlightUnits[i];
                highlight.AttachToUnit(unitToAttach.gameObject);
            }
            Registers[registerIndex][regiment.RegimentID] = newRecordArray;
        }

        //SIMILAIRE MAIS DIFFERENT DE PLACEMENT
        public void ResizeRegister(HighlightRegiment regiment)
        {
            int regimentID = regiment.RegimentID;
            int numUnitsAlive = regiment.CurrentFormation.NumUnitsAlive;
            for (int i = 0; i < Registers.Length; i++)
            {
                CleanUnusedHighlights(i, regimentID, numUnitsAlive);
                ResizeAndReformRegister(i, regiment, numUnitsAlive);
            }
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Orders Callback ◈◈◈◈◈◈                                                                              ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void OnAttackOrderEvent()
        {
        }
    }
}