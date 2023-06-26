using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

using static Unity.Mathematics.math;

namespace KaizerWald
{
    
    public abstract class HighlightCoordinator : MonoBehaviour
    {
        public ulong PlayerID { get; protected set; }
        public PlayerControls HighlightControls { get; protected set; }
        
        [field:Header("Layer Masks")]
        [field:SerializeField] public LayerMask TerrainLayerMask { get; protected set; }
        [field:SerializeField] public LayerMask UnitLayerMask { get; protected set; }
        
        [field:Header("Default Prefabs")]
        [field:SerializeField] public GameObject PreselectionDefaultPrefab { get; protected set; }
        [field:SerializeField] public GameObject SelectionDefaultPrefab { get; protected set; }
        
        public List<SelectableRegiment> SelectableRegiments { get; protected set; }
        public RegimentHighlightSystem RegimentHighlightSystem { get; protected set; }

        // =============================================================================================================
        // -------- Abstract Methods ----------
        // =============================================================================================================
        
        public void RegisterRegiment(SelectableRegiment regiment)
        {
            RegimentHighlightSystem.RegisterRegiment(regiment);
        }
        
        public void UnRegisterRegiment(SelectableRegiment regiment)
        {
            RegimentHighlightSystem.UnregisterRegiment(regiment);
        }
    }
    
}
