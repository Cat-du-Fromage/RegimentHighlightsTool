using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public enum EPlacement : int
    {
        Static,
        Dynamic
    }
    
    public sealed class PlacementSystem : HighlightSystem
    {
        public List<Regiment> SelectedRegiments => MainSystem.Selection.SelectionRegister.ActiveHighlights;
        
        public HighlightRegister StaticPlacementRegister => Registers[0];
        public HighlightRegister DynamicPlacementRegister => Registers[1];

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void InitializeController()
        {
            Debug.Log("Create PlacementController");
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

        public void SwapDynamicToStatic()
        {
            foreach (Regiment regiment in SelectedRegiments)
            {
                int id = regiment.RegimentID;
                for (int i = 0; i < DynamicPlacementRegister.Records[id].Length; i++)
                {
                    Transform dynamicTransform = DynamicPlacementRegister.Records[id][i].transform;
                    StaticPlacementRegister.Records[id][i].transform.SetPositionAndRotation(dynamicTransform.position, dynamicTransform.rotation);
                    /*
                    Vector3 dynamicPosition = DynamicPlacementRegister.Records[id][j].transform.position;
                    Quaternion dynamicRotation = DynamicPlacementRegister.Records[id][j].transform.rotation;
                    StaticPlacementRegister.Records[id][j].transform.SetPositionAndRotation(dynamicPosition, dynamicRotation);
                    */
                }
            }
        }
    }
}
