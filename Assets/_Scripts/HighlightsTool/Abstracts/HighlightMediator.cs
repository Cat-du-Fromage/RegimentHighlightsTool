using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class HighlightMediator : MonoBehaviour
    {
        public ulong PlayerID = 0;
        
        [Header("Layer Masks")]
        [SerializeField] protected LayerMask TerrainLayerMask;
        [SerializeField] protected LayerMask PlayerUnitLayerMask;
        
        public List<ISelectableRegiment> Regiments { get; protected set; }
        public List<HighlightSystem> HighlightSystems { get; protected set; }
        
        
        public void RegisterRegiment(ISelectableRegiment regiment)
        {
            HighlightSystems.ForEach(system => system.AddRegiment(regiment));
        }
        
        public void UnRegisterRegiment(ISelectableRegiment regiment)
        {
            HighlightSystems.ForEach(system => system.RemoveRegiment(regiment));
        }
    }
}