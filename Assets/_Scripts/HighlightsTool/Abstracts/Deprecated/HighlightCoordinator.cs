using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

using static Unity.Mathematics.math;

namespace KaizerWald
{
    /*
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
        [field:SerializeField] public GameObject PlacementDefaultPrefab { get; protected set; }
        
        public HashSet<Regiment> SelectableRegiments { get; protected set; }
        public RegimentHighlightSystem RegimentHighlightSystem { get; protected set; }
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Abstract Methods ◇◇◇◇◇                                                                               ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public void SetPlayerID(ulong playerID) => PlayerID = playerID; //needed later when converting to multiplayer
        
        protected virtual void Awake()
        {
            SelectableRegiments = new HashSet<Regiment>();
            RegimentHighlightSystem = GetComponent<RegimentHighlightSystem>();
            HighlightControls = new PlayerControls();
        }
        
        public virtual void RegisterRegiment(Regiment regiment)
        {
            RegimentHighlightSystem.RegisterRegiment(regiment);
        }
        
        public virtual void UnRegisterRegiment(Regiment regiment)
        {
            RegimentHighlightSystem.UnregisterRegiment(regiment);
        }
    }
    */
}
