using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.Universal;

namespace KaizerWald
{
    public sealed class SelectionBehaviour : HighlightBehaviour
    {
        private PositionConstraint positionConstraint;
        private DecalProjector projector;
        
        private void Awake()
        {
            projector = GetComponent<DecalProjector>();
            Hide();
        }

        public override void InitializeHighlight(Transform unitAttached)
        {
            positionConstraint = GetComponent<PositionConstraint>();
            transform.position = unitAttached.position + Vector3.up;
            positionConstraint.AddSource(new ConstraintSource { sourceTransform = unitAttached });
        }
        
        public override bool IsShown() => projector.enabled == true;
        public override bool IsHidden() => projector.enabled == false;
        
        public override void Show() => projector.enabled = true;
        public override void Hide() => projector.enabled = false;
        
    }
}