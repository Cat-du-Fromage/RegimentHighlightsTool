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
        private RegimentFactory factory;
        public List<Regiment> Regiments { get; private set; }
        public event Action<Regiment> OnNewRegiment; 
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Unity Events ◇◇◇◇◇                                                                                   ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected override void Awake()
        {
            base.Awake();
            Regiments = new List<Regiment>();
            factory = FindObjectOfType<RegimentFactory>();
            //RegimentHighlightSystem.OnOrderReceived;
        }

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◆◆◆◆ Update | Late Update ◆◆◆◆                                                                            │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void Update()
        {
            TestKillUnit();
        }

        private void LateUpdate()
        {
            
        }

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◆◆◆◆ Enable | Disable ◆◆◆◆                                                                                │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
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
        //║ ◇◇◇◇◇ Class Methods ◇◇◇◇◇                                                                                  ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void RegisterNewRegiment(Regiment regiment)
        {
            Regiments.Add(regiment);
            OnNewRegiment?.Invoke(regiment);
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◆◆◆◆ Visibility Trigger ◆◆◆◆                                                                              │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void TestKillUnit()
        {
            if (!Mouse.current.rightButton.wasReleasedThisFrame) return;
            Ray singleRay = Camera.main.ScreenPointToRay(Mouse.current.position.value);
            if (!Physics.Raycast(singleRay, out RaycastHit hit, 1000, 1 << 7)) return;
            DestroyImmediate(hit.transform.gameObject);
        }
    }
}
