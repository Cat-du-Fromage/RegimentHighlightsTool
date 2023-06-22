using UnityEngine.Rendering.Universal;

namespace KaizerWald
{
    public sealed class SelectionBehaviour : HighlightBehaviour
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