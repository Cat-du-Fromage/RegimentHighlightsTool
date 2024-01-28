using System;
using UnityEngine;
using UnityEngine.Animations;

namespace Kaizerwald
{
    public abstract class HighlightBehaviour : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public GameObject UnitAttach { get; protected set; }
        protected Transform UnitTransform => UnitAttach.transform;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public abstract void InitializeHighlight(GameObject unitAttached);

        public virtual void AttachToUnit(GameObject unit) => UnitAttach = unit;

        public abstract void Show();

        public abstract void Hide();
        
        public abstract bool IsShown();

        public abstract bool IsHidden();
    }
}