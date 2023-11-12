using System;
using UnityEngine;
using UnityEngine.Animations;

namespace KaizerWald
{
    public abstract class HighlightBehaviour : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Unit UnitAttach { get; protected set; }
        protected Transform UnitTransform => UnitAttach.transform;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public abstract void InitializeHighlight(Unit unitAttached);

        public virtual void AttachToUnit(Unit unit) => UnitAttach = unit;

        public abstract void Show();

        public abstract void Hide();
        
        public abstract bool IsShown();

        public abstract bool IsHidden();
    }
}