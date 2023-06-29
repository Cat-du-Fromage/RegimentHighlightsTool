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
        
        public HashSet<SelectableRegiment> SelectableRegiments { get; protected set; }
        public RegimentHighlightSystem RegimentHighlightSystem { get; protected set; }

        // =============================================================================================================
        // -------- Abstract Methods ----------
        // =============================================================================================================
        public void SetPlayerID(ulong playerID) => PlayerID = playerID;
        
        protected virtual void Awake()
        {
            SelectableRegiments = new HashSet<SelectableRegiment>();
            RegimentHighlightSystem = GetComponent<RegimentHighlightSystem>();
            HighlightControls = new PlayerControls();
        }
        
        public virtual void RegisterRegiment(SelectableRegiment regiment)
        {
            RegimentHighlightSystem.RegisterRegiment(regiment);
        }
        
        public virtual void UnRegisterRegiment(SelectableRegiment regiment)
        {
            RegimentHighlightSystem.UnregisterRegiment(regiment);
        }
    }
}
