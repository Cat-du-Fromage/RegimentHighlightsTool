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
    public class RegimentManager : HighlightCoordinator
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                          ◆◆◆◆◆◆ STATIC PROPERTIES ◆◆◆◆◆◆                                           ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public static RegimentManager Instance { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private RegimentFactory factory;
        private RegimentHighlightSystem regimentHighlightSystem;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public List<Regiment> Regiments { get; private set; } = new ();
        
        //Allow to retrieve regiment By it's Instance ID
        public Dictionary<int, Regiment> RegimentsByID { get; private set; } = new ();
        
        //Allow to retrieve regiments of a player
        public Dictionary<ulong, List<Regiment>> RegimentsByPlayerID { get; private set; } = new ();
        
        //Allow to retrieve regiments of a team
        public Dictionary<int, List<Regiment>> RegimentsByTeamID { get; private set; } = new ();
        
        public event Action<Regiment> OnNewRegiment;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        protected override void Awake()
        {
            InitializeSingleton();
            base.Awake();
            factory = FindObjectOfType<RegimentFactory>();
            regimentHighlightSystem = GetComponent<RegimentHighlightSystem>();
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                    ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        private void Update()
        {
            foreach (Regiment regiment in Regiments)
            {
                regiment.OnUpdate();
            }
        }

        private void LateUpdate()
        {
            foreach (Regiment regiment in Regiments)
            {
                regiment.OnLateUpdate();
            }
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Enable | Disable ◈◈◈◈◈◈                                                                        ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void OnEnable()
        {
            factory.OnRegimentCreated += RegisterNewRegiment;
            regimentHighlightSystem.OnPlacementEvent += OnMoveOrders;
        }

        private void OnDisable()
        {
            factory.OnRegimentCreated -= RegisterNewRegiment;
            regimentHighlightSystem.OnPlacementEvent -= OnMoveOrders;
            Array.ForEach(OnNewRegiment?.GetInvocationList()!,action => OnNewRegiment -= (Action<Regiment>)action);
            //foreach (Delegate action in OnNewRegiment?.GetInvocationList()!) OnNewRegiment -= (Action<Regiment>)action;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

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

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ PLAYER Highlight Orders ◈◈◈◈◈◈                                                                 ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        //Remplace Par "Order" Generic paramètre List => le tris des ordre est fait Ici
        private void OnMoveOrders(MoveRegimentOrder moveOrder)
        {
            moveOrder.Regiment.StateMachine.OnMoveOrderReceived(moveOrder);
        }
        
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Regiment Update Event ◇◇◇◇◇◇                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void RegisterNewRegiment(Regiment regiment)
        {
            Regiments.Add(regiment);
            
            RegimentsByID.Add(regiment.RegimentID, regiment);
            RegimentsByPlayerID.AddSafe(regiment.OwnerID, regiment);
            RegimentsByTeamID.AddSafe(regiment.TeamID, regiment);
            
            OnNewRegiment?.Invoke(regiment);
        }
        
        /*
        private void TestKillUnit()
        {
            if (!Mouse.current.rightButton.wasReleasedThisFrame) return;
            Ray singleRay = Camera.main.ScreenPointToRay(Mouse.current.position.value);
            if (!Physics.Raycast(singleRay, out RaycastHit hit, 1000, 1 << 7)) return;
            DestroyImmediate(hit.transform.gameObject);
        }
        */


    }
}

//DEBUG A mettre dans "OnMoveOrders(MoveRegimentOrder moveOrder)"
//positionLeader = moveOrder.LeaderDestination;
//directionLeader = moveOrder.FormationDestination.Direction2DForward;
//DEBUG

/*
public bool DebugOrder = false;
private Vector3 positionLeader;
private float2 directionLeader;
private void OnDrawGizmos()
{
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(positionLeader, 0.5f);
    float3 dir2D = new float3(directionLeader.x, 0, directionLeader.y);
    Gizmos.color = Color.yellow;
    Gizmos.DrawLine(positionLeader, (float3)positionLeader + dir2D*2);

    if (Regiments == null) return;
    foreach (Regiment regiment in Regiments)
    {
        Gizmos.color = Color.yellow;
        int regimentId = regiment.RegimentID;
        int width = regiment.CurrentFormation.Width;
        Vector3 pos1 = regimentHighlightSystem.Placement.StaticPlacementRegister[regimentId][0].transform.position;
        Vector3 pos2 = regimentHighlightSystem.Placement.StaticPlacementRegister[regimentId][width-1].transform.position;
        Gizmos.DrawLine(pos1, pos2);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(regiment.transform.position, regiment.transform.position + (Vector3)regiment.CurrentFormation.DirectionForward);
    }
}
*/