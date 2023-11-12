using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
        public event Action<Regiment, Order> OnPlacementEvent;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                                ║
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
        //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                         ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void Update()
        {
            Controllers.ForEach(controller => controller.OnUpdate());
            //foreach (HighlightController controller in Controllers) { controller.OnUpdate(); }
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Enable | Disable ◈◈◈◈◈◈                                                                             ║
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
        public List<Regiment> PreselectedRegiments => Selection.PreselectionRegister.ActiveHighlights;
        public List<Regiment> SelectedRegiments => Selection.SelectionRegister.ActiveHighlights;
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Callback ◈◈◈◈◈◈                                                                                     ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void OnCallback(HighlightSystem system, List<Tuple<Regiment, Order>> orders)
        {
            switch (system)
            {
                case PlacementSystem: // ORDRE
                    //1) Placement-Drag => MoveOrder
                    //2) Placement-NoDrag + No Enemy Preselected => MoveOrder
                    //3) Placement-NoDrag + Enemy Preselected => AttackOrder
                    orders.ForEach(regimentOrder => OnPlacementEvent?.Invoke(regimentOrder.Item1, regimentOrder.Item2));
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
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                       ║
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
        //║ ◈◈◈◈◈◈ Registry Methods ◈◈◈◈◈◈                                                                             ║
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
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ OUTSIDES UPDATES ◈◈◈◈◈◈                                                                             ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void UpdatePlacements(Regiment regiment)
        {
            Placement.UpdateDestinationPlacements(regiment);
        }
        
        public override void ResizeHighlightsRegisters(Regiment regiment, in float3 regimentFuturePosition)
        {
            Selection.ResizeRegister(regiment);
            Placement.ResizeRegister(regiment, regimentFuturePosition);
        }
    }
}
