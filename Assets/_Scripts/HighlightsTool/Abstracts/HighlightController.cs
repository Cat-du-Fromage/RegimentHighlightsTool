using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class HighlightController
    {
        protected Camera PlayerCamera { get; private set; }

        protected HighlightController()
        {
            PlayerCamera = Camera.main;
        }
        
        public abstract void OnEnable();
        public abstract void OnDisable();
        public abstract void OnUpdate();
    }
}
