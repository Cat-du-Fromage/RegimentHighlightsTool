using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

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
        public List<Regiment> PreselectedRegiments => MainSystem.PreselectedRegiments;
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

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Orders Callback ◈◈◈◈◈◈                                                                         ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void OnAttackOrderEvent(Regiment enemyRegiment)
        {
            Debug.Log($"Attack Ordered");
            /*
            AttackOrder order = new AttackOrder(enemyRegiment);
            List<Tuple<Regiment, Order>> attackOrders = new (SelectedRegiments.Count);
            
            foreach (Regiment regiment in SelectedRegiments)
            {
                attackOrders.Add(new Tuple<Regiment, Order>(regiment, order));
            }
            MainSystem.OnCallback(this, attackOrders);
            */
        }
        
        //Callback On RegimentHighlightSystem
        public void OnMoveOrderEvent(int registerIndexUsed, int[] newFormationsWidth)
        {
            Debug.Log($"Move NewFormation Ordered");
            bool keepSameFormation = newFormationsWidth.Length == 0;
            List<Tuple<Regiment, Order>> moveOrders = new (SelectedRegiments.Count);
            
            for (int i = 0; i < SelectedRegiments.Count; i++)
            {
                Regiment regiment = SelectedRegiments[i];
                int width = keepSameFormation ? regiment.CurrentFormation.Width : newFormationsWidth[i];
                MoveOrder order = PackOrder(registerIndexUsed, regiment, width);
                moveOrders.Add(new Tuple<Regiment, Order>(regiment,order));
            }
            MainSystem.OnCallback(this, moveOrders);
        }

        private MoveOrder PackOrder(int registerIndexUsed, Regiment regiment, int width)
        {
            //PROBLEME QUAND UNE UNITE
            float3 firstUnit = Registers[registerIndexUsed][regiment.RegimentID][0].transform.position;
            float3 lastUnit = Registers[registerIndexUsed][regiment.RegimentID][width-1].transform.position;
            float3 direction = normalizesafe(cross(down(), lastUnit - firstUnit));
            
            FormationData formationDestination = new (regiment.CurrentFormation, width, direction);
            float3 leaderDestination = (firstUnit + lastUnit) / 2f;
            MoveOrder order = new MoveOrder(formationDestination, leaderDestination);
            return order;
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
                    Vector3 position = DynamicPlacementRegister[regimentID][i].transform.position;
                    Quaternion rotation = DynamicPlacementRegister[regimentID][i].transform.rotation;
                    StaticPlacementRegister[regimentID][i].transform.SetPositionAndRotation(position, rotation);
                }
            }
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Rearrangement ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void ResizeAndReformRegister(int registerIndex, Regiment regiment, int numHighlightToKeep, in float3 regimentFuturePosition)
        {
            if (!Registers[registerIndex].Records.ContainsKey(regiment.RegimentID)) return;
            HighlightBehaviour[] newRecordArray = Registers[registerIndex][regiment.RegimentID].Slice(0, numHighlightToKeep);
            for (int i = 0; i < numHighlightToKeep; i++)
            {
                HighlightBehaviour highlight = newRecordArray[i];
                Unit unitToAttach = regiment.Units[i];
                highlight.AttachToUnit(unitToAttach);
                
                //Different from Preselection/Selection
                Vector3 position = regiment.CurrentFormation.GetUnitRelativePositionToRegiment3D(i, regimentFuturePosition);
                highlight.transform.position = position;
            }
            Registers[registerIndex][regiment.RegimentID] = newRecordArray;
        }

        //SIMILAIRE MAIS DIFFERENT DE SELECTION
        public void ResizeRegister(Regiment regiment, in float3 regimentFuturePosition)
        {
            int regimentID = regiment.RegimentID;
            int numUnitsAlive = regiment.CurrentFormation.NumUnitsAlive;
            for (int i = 0; i < Registers.Length; i++)
            {
                CleanUnusedHighlights(i, regimentID, numUnitsAlive);
                ResizeAndReformRegister(i, regiment, numUnitsAlive, regimentFuturePosition);
            }
        }
    }
}
