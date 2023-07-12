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
        }

        public override void InitializeHighlight(Transform unitAttached)
        {
            UnitAttach = unitAttached;
            positionConstraint = GetComponent<PositionConstraint>();
            transform.position = unitAttached.position + Vector3.up;
            positionConstraint.AddSource(new ConstraintSource { sourceTransform = unitAttached , weight = 1});
            positionConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            positionConstraint.constraintActive = true;
            positionConstraint.locked = true;
            Hide();
        }
        
        public override bool IsShown() => projector.enabled == true;
        public override bool IsHidden() => projector.enabled == false;
        
        public override void Show()
        {
            positionConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            transform.position = UnitAttach.position + Vector3.up;
            positionConstraint.locked = true;
            projector.enabled = true;
        }

        public override void Hide()
        {
            positionConstraint.locked = false;
            positionConstraint.translationAxis = Axis.None;
            projector.enabled = false;
        }
    }
}