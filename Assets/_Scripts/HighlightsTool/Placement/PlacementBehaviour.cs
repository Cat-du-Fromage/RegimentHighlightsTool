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
            Vector3 position = unitAttached.position + Vector3.up * 0.05f;
            transform.SetPositionAndRotation(position, unitAttached.rotation);
            meshRenderer = GetComponent<MeshRenderer>();
            Hide();
        }

        public override bool IsShown() => meshRenderer.enabled == true;
        public override bool IsHidden() => meshRenderer.enabled == false;

        public override void Show() => meshRenderer.enabled = true;
        public override void Hide() => meshRenderer.enabled = false;
        
    }
}
