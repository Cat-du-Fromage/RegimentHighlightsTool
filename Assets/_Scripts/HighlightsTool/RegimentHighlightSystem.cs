using System;
using System.Collections;
using UnityEngine;

namespace KaizerWald
{
    public abstract class HighlightSystemBehaviour<T> : MonoBehaviour
    where T : HighlightCoordinator
    {
        public abstract T RegimentManager { get; protected set; }
        public HighlightCoordinator Coordinator { get; protected set; }

        public abstract void RegisterRegiment(SelectableRegiment regiment);
        public abstract void UnregisterRegiment(SelectableRegiment regiment);
        
        protected virtual void Awake()
        {
            Coordinator = (HighlightCoordinator)RegimentManager;
        }
    }

    public class RegimentHighlightSystem : HighlightSystemBehaviour<RegimentManager>
    {
        public override RegimentManager RegimentManager { get; protected set; }
        //public HighlightCoordinator Coordinator { get; private set; }
        public SelectionSystem Selection { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Selection = GetComponent<SelectionSystem>();
        }
/*
        public RegimentHighlightSystem(HighlightCoordinator coordinator, GameObject[] prefabs, PlayerControls controls, LayerMask unitLayerMask)
        {
            Coordinator = coordinator;
            Selection = new SelectionSystem(this, prefabs, controls, unitLayerMask);
        }
*/
        public override void RegisterRegiment(SelectableRegiment regiment)
        {
            Selection.AddRegiment(regiment);
        }
        
        public override void UnregisterRegiment(SelectableRegiment regiment)
        {
            Selection.RemoveRegiment(regiment);
        }
    }
}
