using System;
using System.Collections;
using System.Linq;
using UnityEditor.Rendering;

namespace KaizerWald
{
    public sealed class RegimentHighlightSystem : HighlightSystemBehaviour<RegimentManager>
    {
        public override RegimentManager RegimentManager { get; protected set; }
        public SelectionSystem Selection { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Selection = this.GetOrAddComponent<SelectionSystem>();
        }

        private void OnEnable()
        {
            RegimentManager.OnNewRegiment += PopulateHighlights;
        }

        private void OnDisable()
        {
            RegimentManager.OnNewRegiment -= PopulateHighlights;
        }

        public override void RegisterRegiment(SelectableRegiment regiment)
        {
            base.RegisterRegiment(regiment);
            Selection.AddRegiment(regiment);
        }
        
        public override void UnregisterRegiment(SelectableRegiment regiment)
        {
            base.UnregisterRegiment(regiment);
            Selection.RemoveRegiment(regiment);
        }
    }
}
