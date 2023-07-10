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
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private RegimentFactory factory;
        private RegimentHighlightSystem regimentHighlightSystem;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public static RegimentManager Instance { get; private set; }
        public List<Regiment> Regiments { get; private set; }
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
            Regiments = new List<Regiment>();
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
            Debug.Log($"Regiment Manager Pass Order: {moveOrder.FormationDestination.ToString()}");
            //DEBUG
            positionLeader = moveOrder.LeaderDestination;
            directionLeader = moveOrder.FormationDestination.Direction2DForward();
            //DEBUG
            moveOrder.Regiment.OnMoveOrderReceived(moveOrder);
        }
        
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Regiment Update Event ◇◇◇◇◇◇                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void RegisterNewRegiment(Regiment regiment)
        {
            Regiments.Add(regiment);
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
                Gizmos.DrawLine(regiment.transform.position, regiment.transform.position + (Vector3)regiment.CurrentFormation.Direction3DForward());
            }
            
            //regimentHighlightSystem.Placement.StaticPlacementRegister
            //Gizmos.DrawLine(, (float3)positionLeader + dir2D*2);
        }
    }
}
