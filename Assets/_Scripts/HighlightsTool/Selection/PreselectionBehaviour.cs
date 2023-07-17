using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.Universal;

namespace KaizerWald
{
    public sealed class PreselectionBehaviour : HighlightBehaviour
    {
        private PositionConstraint positionConstraint;
        private DecalProjector projector;
        
        private void Awake()
        {
            projector = GetComponent<DecalProjector>();
        }

        public override void InitializeHighlight(Unit unitAttached)
        {
            positionConstraint = GetComponent<PositionConstraint>();
            AttachToUnit(unitAttached);
            Hide();
        }

        public override void AttachToUnit(Unit unit)
        {
            base.AttachToUnit(unit);
            UnlockConstraint();
            transform.position = UnitTransform.position + Vector3.up;
            ConstraintSource source = new ConstraintSource { sourceTransform = UnitTransform, weight = 1 };
            if (positionConstraint.sourceCount == 0)
            {
                positionConstraint.AddSource(source);
            }
            else
            {
                positionConstraint.SetSource(0, source);
            }
            //positionConstraint.AddSource(new ConstraintSource { sourceTransform = UnitTransform , weight = 1});
            positionConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            LockConstraint();
        }

        private void LockConstraint()
        {
            positionConstraint.constraintActive = true;
            positionConstraint.locked = true;
        }
        
        private void UnlockConstraint()
        {
            positionConstraint.constraintActive = false;
            positionConstraint.locked = false;
        }

        public override bool IsShown() => projector.enabled == true;

        public override bool IsHidden() => projector.enabled == false;

        public override void Show()
        {
            positionConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            transform.position = UnitTransform.position + Vector3.up;
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