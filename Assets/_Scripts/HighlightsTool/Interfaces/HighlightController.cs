using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class HighlightController
    {
        protected HighlightSystem HighlightSystem { get; private set; }
        protected Camera PlayerCamera { get; private set; }

        protected HighlightController(HighlightSystem system, Camera camera)
        {
            HighlightSystem = system;
            PlayerCamera = camera;
        }
        
        public abstract void OnEnable();
        public abstract void OnDisable();
    }
}
