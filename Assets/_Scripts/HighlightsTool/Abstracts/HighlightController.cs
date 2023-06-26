using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class HighlightController
    {
        protected HighlightSystem HighlightSystem { get; private set; }
        protected Camera PlayerCamera { get; private set; }

        protected HighlightController(HighlightSystem system)
        {
            HighlightSystem = system;
            PlayerCamera = Camera.main;
        }
        
        public abstract void OnEnable();
        public abstract void OnDisable();
    }
}
