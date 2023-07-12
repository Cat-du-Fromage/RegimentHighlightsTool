using System;
using UnityEngine;
using UnityEngine.Animations;

namespace KaizerWald
{
    public abstract class HighlightBehaviour : MonoBehaviour
    {
        protected Transform UnitAttach;
        public abstract void InitializeHighlight(Transform unitAttached);

        public abstract void Show();

        public abstract void Hide();
        
        public abstract bool IsShown();

        public abstract bool IsHidden();
    }
}