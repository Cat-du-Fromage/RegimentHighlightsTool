using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;

namespace KaizerWald
{
    public sealed class RegimentHighlightSystem : HighlightSystemBehaviour<RegimentManager>
    {
        public override RegimentManager RegimentManager { get; protected set; }
        public SelectionSystem Selection { get; private set; }
        public PlacementSystem Placement { get; private set; }

        private List<HighlightController> Controllers = new List<HighlightController>();
        
        //OnHoverUpdate
        //OnSelectionUpdate
        //OnPlacementUpdate

        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Unity Events ◇◇◇◇◇                                                                                   ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected override void Awake()
        {
            base.Awake();
            Selection = this.GetOrAddComponent<SelectionSystem>();
            Placement = this.GetOrAddComponent<PlacementSystem>();
            Controllers = new List<HighlightController>() { Selection.Controller, Placement.Controller };
        }

        private void Update()
        {
            if (Controllers.Count is 0) return;
            foreach (HighlightController controller in Controllers)
            {
                controller.OnUpdate();
            }
        }

        private void OnEnable()
        {
            RegimentManager.OnNewRegiment += RegisterRegiment;
            Controllers?.ForEach(controller => controller.OnEnable());
        }

        private void OnDisable()
        {
            RegimentManager.OnNewRegiment -= RegisterRegiment;
            Controllers?.ForEach(controller => controller.OnDisable());
        }
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Class Methods ◇◇◇◇◇                                                                                  ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override void RegisterRegiment(Regiment regiment)
        {
            base.RegisterRegiment(regiment);
            Selection.AddRegiment(regiment);
            Placement.AddRegiment(regiment);
        }
        
        public override void UnregisterRegiment(Regiment regiment)
        {
            base.UnregisterRegiment(regiment);
            Selection.RemoveRegiment(regiment);
            Placement.RemoveRegiment(regiment);
        }
    }
}
