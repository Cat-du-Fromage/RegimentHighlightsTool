using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    public sealed class PlacementSystem : HighlightSystem
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public static readonly int StaticRegisterIndex = 0;
        public static readonly int DynamicRegisterIndex = 1;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public List<Regiment> SelectedRegiments => MainSystem.SelectedRegiments;
        public HighlightRegister StaticPlacementRegister => Registers[StaticRegisterIndex];
        public HighlightRegister DynamicPlacementRegister => Registers[DynamicRegisterIndex];

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

        //Callback On RegimentHighlightSystem
        public void OnMoveOrderEvent(int[] newFormationsWidth)
        {
            bool keepSameFormation = newFormationsWidth.Length == 0;
            List<RegimentOrder> orders = new (SelectedRegiments.Count);
            
            List<MoveRegimentOrder> moveOrders = new (SelectedRegiments.Count);
            
            for (int i = 0; i < SelectedRegiments.Count; i++)
            {
                Regiment regiment = SelectedRegiments[i];
                int width = keepSameFormation ? regiment.CurrentFormation.Width : newFormationsWidth[i];
                Vector3 firstUnit = StaticPlacementRegister.Records[regiment.RegimentID][0].transform.position;
                Vector3 lastUnit = StaticPlacementRegister.Records[regiment.RegimentID][width-1].transform.position;
                MoveRegimentOrder order = new MoveRegimentOrder(regiment, EStates.Move, width, firstUnit, lastUnit);
                orders.Add(order);
                moveOrders.Add(order);
            }
            MainSystem.OnCallback(this, orders);
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        protected override void InitializeController()
        {
            Controller = new PlacementController(this, Coordinator.HighlightControls, Coordinator.TerrainLayerMask);
        }

        protected override void InitializeRegisters()
        {
            GameObject[] prefabs = new[] { Coordinator.PlacementDefaultPrefab, Coordinator.PlacementDefaultPrefab };
            for (int i = 0; i < prefabs.Length; i++)
            {
                Registers[i] = new HighlightRegister(this, prefabs[i]);
            }
        }

        public override void AddRegiment(Regiment regiment)
        {
            if (regiment.OwnerID != Coordinator.PlayerID) return;
            base.AddRegiment(regiment);
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Regiment Update Event ◈◈◈◈◈◈                                                                   ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void SwapDynamicToStatic()
        {
            foreach (Regiment regiment in SelectedRegiments)
            {
                int regimentID = regiment.RegimentID;
                for (int i = 0; i < DynamicPlacementRegister.Records[regimentID].Length; i++)
                {
                    Vector3 position = DynamicPlacementRegister.Records[regimentID][i].transform.position;
                    Quaternion rotation = DynamicPlacementRegister.Records[regimentID][i].transform.rotation;
                    StaticPlacementRegister.Records[regimentID][i].transform.SetPositionAndRotation(position, rotation);
                }
            }
        }
    }
}
