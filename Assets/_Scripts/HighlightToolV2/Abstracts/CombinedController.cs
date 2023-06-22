using UnityEngine;

namespace KaizerWald2
{
    public abstract class CombinedController
    {
        protected CompositeSystem CompositeSystem { get; private set; }
        protected Camera PlayerCamera { get; private set; }

        protected CombinedController(CompositeSystem compositeSystem, Camera camera)
        {
            CompositeSystem = compositeSystem;
            PlayerCamera = camera;
        }
        
        public abstract void OnEnable();
        
        public abstract void OnDisable();
    }
}