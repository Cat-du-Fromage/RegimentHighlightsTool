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
        public List<Regiment> SelectedRegiment => Selection.SelectionRegister.ActiveHighlights;

        // =============================================================================================================
        // ----- Unity Events -----
        // =============================================================================================================
        protected override void Awake()
        {
            base.Awake();
            Selection = this.GetOrAddComponent<SelectionSystem>();
            Placement = this.GetOrAddComponent<PlacementSystem>();
        }

        private void OnEnable()
        {
            RegimentManager.OnNewRegiment += RegisterRegiment;
        }

        private void OnDisable()
        {
            RegimentManager.OnNewRegiment -= RegisterRegiment;
        }
        
        // =============================================================================================================
        // ----- Class Methods -----
        // =============================================================================================================

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
