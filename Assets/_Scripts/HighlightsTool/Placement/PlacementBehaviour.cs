using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class PlacementBehaviour : HighlightBehaviour
    {
        private MeshRenderer meshRenderer;
        
        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            Hide();
        }

        public override void InitializeHighlight(Transform unitAttached)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            Hide();
        }

        public override bool IsShown() => meshRenderer.enabled == true;
        public override bool IsHidden() => meshRenderer.enabled == false;

        public override void Show() => meshRenderer.enabled = true;
        public override void Hide() => meshRenderer.enabled = false;
        
    }
}
