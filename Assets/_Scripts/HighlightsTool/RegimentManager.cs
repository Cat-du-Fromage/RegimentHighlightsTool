using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace KaizerWald
{
    public class RegimentManager : HighlightCoordinator
    {
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║                                            ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                             ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private RegimentFactory factory;
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║                                          ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                          ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public static RegimentManager Instance { get; private set; }
        public List<Regiment> Regiments { get; private set; }
        public event Action<Regiment> OnNewRegiment;

        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║                                         ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                         ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        protected override void Awake()
        {
            InitSingleton();
            base.Awake();
            Regiments = new List<Regiment>();
            factory = FindObjectOfType<RegimentFactory>();
            Debug.Log($"sizeof(FormationData): {Utilities.GetSizeOf<FormationData>()}");
            //RegimentHighlightSystem.OnOrderReceived;
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
        }

        private void OnDisable()
        {
            factory.OnRegimentCreated -= RegisterNewRegiment;
            foreach (Delegate action in OnNewRegiment?.GetInvocationList()!)
            {
                OnNewRegiment -= (Action<Regiment>)action;
            }
        }
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║                                        ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                         ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void InitSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Regiment Update Event ◇◇◇◇◇◇                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void RegisterNewRegiment(Regiment regiment)
        {
            Regiments.Add(regiment);
            OnNewRegiment?.Invoke(regiment);
        }

        private void OnOrderCallback(HighlightSystem systemSource, BaseOrder[] orders)
        {
            switch (systemSource)
            {
                case PlacementSystem:
                    foreach (BaseOrder baseOrder in orders)
                    {
                        MoveOrder moveOrder = (MoveOrder)baseOrder;
                    }
                    return;
            }
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
