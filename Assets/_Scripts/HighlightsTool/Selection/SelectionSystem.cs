using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KaizerWald
{
    public sealed class SelectionSystem : HighlightSystem
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public static readonly int PreselectionRegisterIndex = 0;
        public static readonly int SelectionRegisterIndex = 1;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public List<Regiment> Regiments => MainSystem.RegimentManager.RegimentsByPlayerID[MainSystem.RegimentManager.PlayerID];
        public HighlightRegister PreselectionRegister => Registers[PreselectionRegisterIndex];
        public HighlightRegister SelectionRegister => Registers[SelectionRegisterIndex];
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        protected override void Awake()
        {
            base.Awake();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
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
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Behaviours Methods ◈◈◈◈◈◈                                                                      ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
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
            foreach (Regiment regiment in Registers[registerIndex].ActiveHighlights)
            {
                ((ISelectable)regiment).SetSelectableProperty(registerIndex, false);
            }
            //Registers[registerIndex].ActiveHighlights.ForEach(regiment => ((ISelectable)regiment).SetSelectableProperty(registerIndex, false));
            base.HideAll(registerIndex);
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Orders Callback ◈◈◈◈◈◈                                                                         ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void OnAttackOrderEvent()
        {
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Rearrangement ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void ResizeAndReformRegister(int registerIndex, Regiment regiment, int numHighlightToKeep)
        {
            if (!Registers[registerIndex].Records.ContainsKey(regiment.RegimentID)) return;
            HighlightBehaviour[] newRecordArray = Registers[registerIndex][regiment.RegimentID].Slice(0, numHighlightToKeep);
            for (int i = 0; i < numHighlightToKeep; i++)
            {
                HighlightBehaviour highlight = newRecordArray[i];
                Unit unitToAttach = regiment.Units[i];
                highlight.AttachToUnit(unitToAttach);
            }
            Registers[registerIndex][regiment.RegimentID] = newRecordArray;
        }

        //SIMILAIRE MAIS DIFFERENT DE PLACEMENT
        public void ResizeRegister(Regiment regiment)
        {
            int regimentID = regiment.RegimentID;
            int numUnitsAlive = regiment.CurrentFormation.NumUnitsAlive;
            for (int i = 0; i < Registers.Length; i++)
            {
                CleanUnusedHighlights(i, regimentID, numUnitsAlive);
                ResizeAndReformRegister(i, regiment, numUnitsAlive);
            }
        }
    }
}