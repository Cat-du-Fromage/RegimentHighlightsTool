using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace KaizerWald
{
    public sealed class Preselection : HighlightBehaviour
    {
        private DecalProjector projector;
        
        protected override void Awake()
        {
            base.Awake();
            projector = GetComponent<DecalProjector>();
            Hide();
        }

        public override void Hide() => projector.enabled = false;
        public override void Show() => projector.enabled = true;
    }
}
