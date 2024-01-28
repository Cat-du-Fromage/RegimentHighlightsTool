using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Kaizerwald
{
    //FAIRE de régiment manager une partie intégrante de l'outil "HighlightRegimentManager"
    public partial class HighlightRegimentManager : MonoBehaviourSingleton<HighlightRegimentManager>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //private RegimentFactory factory;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] public ulong PlayerID { get; private set; }
        
        [field:Header("Layer Masks")]
        [field:SerializeField] public LayerMask TerrainLayerMask { get; private set; }
        [field:SerializeField] public LayerMask UnitLayerMask { get; private set; }
        
        [field:Header("Default Prefabs")]
        [field:SerializeField] public GameObject PreselectionDefaultPrefab { get; private set; }
        [field:SerializeField] public GameObject SelectionDefaultPrefab { get; private set; }
        [field:SerializeField] public GameObject PlacementDefaultPrefab { get; private set; }
        
        public PlayerControls HighlightControls { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Highlights ◈◈◈◈◈◈                                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public SelectionSystem Selection { get; private set; }
        public PlacementSystem Placement { get; private set; }
        
        private List<HighlightController> Controllers = new (2);
        public List<HighlightRegiment> PreselectedRegiments => Selection.PreselectionRegister.ActiveHighlights;
        public List<HighlightRegiment> SelectedRegiments => Selection.SelectionRegister.ActiveHighlights;
        
        public event Action OnSelectionEvent;
        public event Action<GameObject, Order> OnPlacementEvent;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Containers ◈◈◈◈◈◈                                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public List<HighlightRegiment> Regiments { get; private set; } = new ();
        
        //Allow to retrieve regiment By it's Instance ID
        public Dictionary<int, HighlightRegiment> RegimentsByID { get; private set; } = new ();
        
        //Allow to retrieve regiments of a player
        public Dictionary<ulong, List<HighlightRegiment>> RegimentsByPlayerID { get; private set; } = new ();
        
        //Allow to retrieve regiments of a team
        public Dictionary<int, List<HighlightRegiment>> RegimentsByTeamID { get; private set; } = new ();
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        protected override void Awake()
        {
            base.Awake();
            //factory = FindFirstObjectByType<RegimentFactory>();
            
            HighlightControls = new PlayerControls();
            Selection = new SelectionSystem(this);
            Placement = new PlacementSystem(this);
            Controllers = new List<HighlightController>() { Selection.Controller, Placement.Controller };
        }
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void Update()
        {
            Controllers.ForEach(controller => controller.OnUpdate());
        }

        private void LateUpdate()
        {
            CleanupEmptyRegiments();
        }

        private void CleanupEmptyRegiments()
        {
            if (Regiments.Count == 0) return;
            for (int i = Regiments.Count - 1; i > -1; i--)
            {
                HighlightRegiment regiment = Regiments[i];
                if (regiment.CurrentFormation.NumUnitsAlive > 0) continue;
                Debug.Log($"Regiment destroyed: {regiment.name}");
                UnRegisterRegiment(regiment);
                Destroy(regiment);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Enable | Disable ◈◈◈◈◈◈                                                                                 ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        public void OnEnable()
        {
            Controllers?.ForEach(controller => controller.OnEnable());
        }

        public void OnDisable()
        {
            Controllers?.ForEach(controller => controller.OnDisable());
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void SetPlayerID(ulong playerID) => PlayerID = playerID;
        
        public bool RegimentExist(int regimentID) => RegimentsByID.ContainsKey(regimentID);

        public int GetEnemiesTeamNumUnits(int friendlyTeamID)
        {
            int numUnits = 0;
            foreach ((int teamID, List<HighlightRegiment> regiments) in RegimentsByTeamID)
            {
                if (teamID == friendlyTeamID) continue;
                numUnits += regiments.Count;
            }
            return numUnits;
        }

        public int GetTeamNumUnits(int searchedTeamID)
        {
            int numUnits = 0;
            foreach ((int teamId, List<HighlightRegiment> regiments) in RegimentsByTeamID)
            {
                if (teamId != searchedTeamID) continue;
                numUnits += regiments.Count;
            }
            return numUnits;
        }
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Regiment Update Event ◇◇◇◇◇◇                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

        public void RegisterRegiment<T>(ulong ownerID, int teamID, GameObject regimentGameObject, List<T> units, int2 minMaxRow, float2 unitSize, float spaceBetweenUnit, float3 direction) 
        where T : MonoBehaviour
        {
            HighlightRegiment newHighlightRegiment = regimentGameObject.AddComponent<HighlightRegiment>();
            newHighlightRegiment.InitializeProperties(ownerID, teamID, units, minMaxRow, unitSize, spaceBetweenUnit, direction);
            RegisterRegiment(newHighlightRegiment, units);
        }
        
        public void RegisterRegiment<T>(HighlightRegiment regiment, List<T> units) where T : MonoBehaviour
        {
            Regiments.Add(regiment);
            RegimentsByID.TryAdd(regiment.RegimentID, regiment);
            RegimentsByPlayerID.AddSafe(regiment.OwnerID, regiment);
            RegimentsByTeamID.AddSafe(regiment.TeamID, regiment);
            
            Selection.AddRegiment(regiment,units);
            Placement.AddRegiment(regiment,units);
        }
        
        public void UnRegisterRegiment(HighlightRegiment regiment)
        {
            Selection.RemoveRegiment(regiment);
            Placement.RemoveRegiment(regiment);
            
            Regiments.Remove(regiment);
            RegimentsByID.Remove(regiment.RegimentID);
            RegimentsByPlayerID[regiment.OwnerID].Remove(regiment);
            RegimentsByTeamID[regiment.TeamID].Remove(regiment);
        }
        
        //TODO: Rework callbacks, il faut quelque chose de plus général!
        public void OnCallback(HighlightSystem system, List<Tuple<GameObject, Order>> orders)
        {
            switch (system)
            {
                case PlacementSystem: // ORDRE
                    //1) Placement-Drag => MoveOrder
                    //2) Placement-NoDrag + No Enemy Preselected => MoveOrder
                    //3) Placement-NoDrag + Enemy Preselected => AttackOrder
                    foreach ((GameObject regiment, Order order) in orders)
                    {
                        //OnPlayerOrder(regiment, order);
                        OnPlacementEvent?.Invoke(regiment, order);
                    }
                    return;
                case SelectionSystem: // Indication (UI Regiment Preselected)
                    OnSelectionEvent?.Invoke();
                    return;
                default:
                    return;
            }
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ OUTSIDES UPDATES ◈◈◈◈◈◈                                                                             ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void UpdatePlacements(HighlightRegiment regiment)
        {
            Placement.UpdateDestinationPlacements(regiment);
        }
        
        public void ResizeHighlightsRegisters(HighlightRegiment regiment, in float3 regimentFuturePosition)
        {
            Selection.ResizeRegister(regiment);
            Placement.ResizeRegister(regiment, regimentFuturePosition);
        }
    }
}