using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;

namespace KaizerWald
{
    public sealed class RegimentHighlightSystem : HighlightSystemBehaviour<RegimentManager>
    {
        private static RegimentHighlightSystem instance;
        public static RegimentHighlightSystem Instance
        {
            get
            {
                if (!instance) instance = FindFirstObjectByType<RegimentHighlightSystem>();
                return instance;
            }
        }
        //public static RegimentHighlightSystem Instance { get; private set; }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private List<HighlightController> Controllers = new List<HighlightController>();
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public override RegimentManager RegimentManager { get; protected set; }
        public SelectionSystem Selection { get; private set; }
        public PlacementSystem Placement { get; private set; }

        public event Action OnSelectionEvent;
        public event Action<MoveRegimentOrder> OnPlacementEvent;

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
            Selection = this.GetOrAddComponent<SelectionSystem>();
            Placement = this.GetOrAddComponent<PlacementSystem>();
            Controllers = new List<HighlightController>() { Selection.Controller, Placement.Controller };
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                    ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void Update()
        {
            if (Controllers.Count is 0) return;
            foreach (HighlightController controller in Controllers)
            {
                controller.OnUpdate();
            }
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Enable | Disable ◈◈◈◈◈◈                                                                        ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void OnEnable()
        {
            Controllers?.ForEach(controller => controller.OnEnable());
        }

        private void OnDisable()
        {
            Controllers?.ForEach(controller => controller.OnDisable());
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public List<Regiment> SelectedRegiments => Selection.SelectionRegister.ActiveHighlights;
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Callback ◈◈◈◈◈◈                                                                                ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void OnCallback(HighlightSystem system, List<RegimentOrder> orders)
        {
            switch (system)
            {
                case PlacementSystem: // ORDRE
                    if (orders.Count == 0) return;
                    //1) Placement-Drag => MoveOrder
                    //2) Placement-NoDrag + No Enemy Preselected => MoveOrder
                    if (orders[0] is MoveRegimentOrder)
                    {
                        foreach (RegimentOrder order in orders)
                        {
                            OnPlacementEvent?.Invoke((MoveRegimentOrder)order);
                        }
                    }
                    //3) Placement-NoDrag + Enemy Preselected => AttackOrder
                    return;
                case SelectionSystem: // Indication (UI Regiment Preselected)
                    OnSelectionEvent?.Invoke();
                    return;
                default:
                    return;
            }
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void InitializeSingleton()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }
            instance = this;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Registry Methods ◈◈◈◈◈◈                                                                        ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public override void RegisterRegiment(Regiment regiment)
        {
            Selection.AddRegiment(regiment);
            Placement.AddRegiment(regiment);
        }
        
        public override void UnregisterRegiment(Regiment regiment)
        {
            Selection.RemoveRegiment(regiment);
            Placement.RemoveRegiment(regiment);
        }
        
        public override void ResizeBuffers(int regimentID, int numDead)
        {
            //SELECTION ARE SPECIAL!
            Placement.ResizeBuffer(regimentID, numDead);
        }
    }
}
