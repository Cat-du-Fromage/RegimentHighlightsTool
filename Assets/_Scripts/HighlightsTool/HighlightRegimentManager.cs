using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace KaizerWald
{
    //FAIRE de régiment manager une partie intégrante de l'outil "HighlightRegimentManager"
    public partial class HighlightRegimentManager : MonoBehaviourSingleton<HighlightRegimentManager>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private RegimentFactory factory;
        
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
        
        private List<HighlightController> Controllers = new List<HighlightController>();
        public List<Regiment> PreselectedRegiments => Selection.PreselectionRegister.ActiveHighlights;
        public List<Regiment> SelectedRegiments => Selection.SelectionRegister.ActiveHighlights;
        
        public event Action OnSelectionEvent;
        public event Action<Regiment, Order> OnPlacementEvent;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Containers ◈◈◈◈◈◈                                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public List<Regiment> Regiments { get; private set; } = new ();
        
        //Allow to retrieve regiment By it's Instance ID
        public Dictionary<int, Regiment> RegimentsByID { get; private set; } = new ();
        
        //Allow to retrieve regiments of a player
        public Dictionary<ulong, List<Regiment>> RegimentsByPlayerID { get; private set; } = new ();
        
        //Allow to retrieve regiments of a team
        public Dictionary<int, List<Regiment>> RegimentsByTeamID { get; private set; } = new ();
        
        public event Action<Regiment> OnNewRegiment;
        public event Action<Regiment> OnDeadRegiment;
        
        private List<Tuple<Regiment, Order>> Orders = new (10);
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        protected override void Awake()
        {
            base.Awake();
            factory = FindFirstObjectByType<RegimentFactory>();
            
            HighlightControls = new PlayerControls();
            Selection = new SelectionSystem(this);
            Placement = new PlacementSystem(this);
            Controllers = new List<HighlightController>() { Selection.Controller, Placement.Controller };
        }
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void FixedUpdate()
        {
            //OUT OF PLACE!
            Regiments.ForEach(regiment => regiment.OnFixedUpdate());
        }

        private void Update()
        {
            Controllers.ForEach(controller => controller.OnUpdate());
            
            //OUT OF PLACE!
            ProcessOrders();
            Regiments.ForEach(regiment => regiment.OnUpdate());
        }

        private void LateUpdate()
        {
            CleanupEmptyRegiments();
            
            //OUT OF PLACE!
            Regiments.ForEach(regiment => regiment.OnLateUpdate());
        }

        private void CleanupEmptyRegiments()
        {
            if (Regiments.Count == 0) return;
            for (int i = Regiments.Count - 1; i > -1; i--)
            {
                Regiment regiment = Regiments[i];
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
            factory.OnRegimentCreated += RegisterRegiment;
        }

        public void OnDisable()
        {
            Controllers?.ForEach(controller => controller.OnDisable());
            factory.OnRegimentCreated -= RegisterRegiment;
            OnNewRegiment?.GetInvocationList().ForEachSafe(action => OnNewRegiment -= (Action<Regiment>)action);
            OnDeadRegiment?.GetInvocationList().ForEachSafe(action => OnDeadRegiment -= (Action<Regiment>)action);
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void SetPlayerID(ulong playerID) => PlayerID = playerID;
        
        public bool RegimentExist(int regimentID) => RegimentsByID.ContainsKey(regimentID);

        public int GetEnemiesTeamNumUnits(int friendlyTeamID)
        {
            int numUnits = 0;
            foreach ((int teamID, List<Regiment> regiments) in RegimentsByTeamID)
            {
                if (teamID == friendlyTeamID) continue;
                numUnits += regiments.Count;
            }
            return numUnits;
        }

        public int GetTeamNumUnits(int searchedTeamID)
        {
            int numUnits = 0;
            foreach ((int teamId, List<Regiment> regiments) in RegimentsByTeamID)
            {
                if (teamId != searchedTeamID) continue;
                numUnits += regiments.Count;
            }
            return numUnits;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ PLAYER Highlight Orders ◈◈◈◈◈◈                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void ProcessOrders()
        {
            foreach ((Regiment regiment, Order order) in Orders)
            {
                regiment.BehaviourTree.OnOrderReceived(order);
            }
            Orders.Clear();
        }
    
        //Remplace Par "Order" Generic paramètre List => le tris des ordre est fait Ici
        private void OnPlayerOrder(Regiment regiment, Order regimentMoveOrder)
        {
            Orders.Add(new Tuple<Regiment, Order>(regiment, regimentMoveOrder));
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Regiment Update Event ◇◇◇◇◇◇                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

        public void RegisterRegiment(Regiment regiment)
        {
            Regiments.Add(regiment);
            RegimentsByID.TryAdd(regiment.RegimentID, regiment);
            RegimentsByPlayerID.AddSafe(regiment.OwnerID, regiment);
            RegimentsByTeamID.AddSafe(regiment.TeamID, regiment);
            
            Selection.AddRegiment(regiment);
            Placement.AddRegiment(regiment);
            OnNewRegiment?.Invoke(regiment); //MAYBE USELESS
        }
        
        public void UnRegisterRegiment(Regiment regiment)
        {
            OnDeadRegiment?.Invoke(regiment); //MAYBE USELESS
            Selection.RemoveRegiment(regiment);
            Placement.RemoveRegiment(regiment);
            
            Regiments.Remove(regiment);
            RegimentsByID.Remove(regiment.RegimentID);
            RegimentsByPlayerID[regiment.OwnerID].Remove(regiment);
            RegimentsByTeamID[regiment.TeamID].Remove(regiment);
        }
        
        public void OnCallback(HighlightSystem system, List<Tuple<Regiment, Order>> orders)
        {
            switch (system)
            {
                case PlacementSystem: // ORDRE
                    //1) Placement-Drag => MoveOrder
                    //2) Placement-NoDrag + No Enemy Preselected => MoveOrder
                    //3) Placement-NoDrag + Enemy Preselected => AttackOrder
                    foreach ((Regiment regiment, Order order) in orders)
                    {
                        OnPlayerOrder(regiment, order);
                        OnPlacementEvent?.Invoke(regiment, order);
                    }
                    //orders.ForEach(regimentOrder => OnPlacementEvent?.Invoke(regimentOrder.Item1, regimentOrder.Item2));
                    //foreach ((Regiment regiment, Order order) in orders) OnPlacementEvent?.Invoke(regiment, order);
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
        
        public void UpdatePlacements(Regiment regiment)
        {
            Placement.UpdateDestinationPlacements(regiment);
        }
        
        public void ResizeHighlightsRegisters(Regiment regiment, in float3 regimentFuturePosition)
        {
            Selection.ResizeRegister(regiment);
            Placement.ResizeRegister(regiment, regimentFuturePosition);
        }
    }
}