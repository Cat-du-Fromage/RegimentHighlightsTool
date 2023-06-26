using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class HighlightMediator : MonoBehaviour
    {
        public ulong PlayerID = 0;
        
        [Header("Layer Masks")]
        [SerializeField] protected LayerMask TerrainLayerMask;
        [SerializeField] protected LayerMask UnitLayerMask;
        
        public List<SelectableRegiment> Regiments { get; protected set; }
        public List<HighlightSystem> HighlightSystems { get; protected set; }
        
        
        public void RegisterRegiment(SelectableRegiment regiment)
        {
            HighlightSystems.ForEach(system => system.AddRegiment(regiment));
        }
        
        public void UnRegisterRegiment(SelectableRegiment regiment)
        {
            HighlightSystems.ForEach(system => system.RemoveRegiment(regiment));
        }
    }
}