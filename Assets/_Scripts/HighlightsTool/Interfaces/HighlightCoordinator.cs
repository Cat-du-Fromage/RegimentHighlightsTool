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
        public ulong PlayerID = 0;

        private PlayerControls HighlightControls;
        
        [Header("Layer Masks")]
        [SerializeField] private LayerMask TerrainLayerMask;
        [SerializeField] private LayerMask PlayerUnitLayerMask;
        
        [Header("Default Prefabs")]
        [SerializeField] private GameObject PreselectionDefaultPrefab;
        [SerializeField] private GameObject SelectionDefaultPrefab;
        
        public List<ISelectableRegiment> Regiments { get; protected set; }
        public List<HighlightSystem> HighlightSystems { get; protected set; }
        
        public PreselectionSystem Preselection { get; private set; }
        public SelectionSystem Selection { get; private set; }
        
        public event Action<ISelectableRegiment> OnSinglePreselectionEvent;
        public event Action OnSelectionEvent;

        // =============================================================================================================
        // -------- Unity Events ----------
        // =============================================================================================================
        
        protected virtual void Awake()
        {
            HighlightControls = new PlayerControls();
            HighlightControls.Enable();
            
            Preselection = new PreselectionSystem(this, PreselectionDefaultPrefab, PlayerUnitLayerMask, HighlightControls);
            Selection = new SelectionSystem(this, SelectionDefaultPrefab, HighlightControls, Preselection);

            HighlightSystems = new() { Preselection, Selection };
        }

        protected virtual void Start()
        {
            Regiment[] regiments = FindObjectsOfType<Regiment>();
            if (regiments.Length == 0)
            {
                Debug.Log("No SelectableRegiments Found");
                return;
            }

            Regiments = new List<ISelectableRegiment>(regiments.Length);
            foreach (Regiment regiment in regiments)
            {
                if (!regiment.TryGetComponent(out ISelectableRegiment selectableRegiment)) continue;
                if (selectableRegiment.OwnerID != PlayerID) continue;

                //TODO: NOT GOOD HERE NEED REFACTOR!
                Array.ForEach(regiment.UnitsTransform, unit => unit.gameObject.layer = floorlog2(PlayerUnitLayerMask));

                Regiments.Add(selectableRegiment);
                RegisterRegiment(regiment);
            }
        }
        
        // =============================================================================================================
        // -------- Events ----------
        // =============================================================================================================
        
        public void OnNotification(HighlightSystem system)
        {
            switch (system)
            {
                case PreselectionSystem:
                    OnSinglePreselectionEvent?.Invoke(Preselection.PreselectedRegiment[0]);
                    break;
                case SelectionSystem:
                    OnSelectionEvent?.Invoke();
                    break;
                default:
                    break;
            }
        }

        // =============================================================================================================
        // -------- Abstract Methods ----------
        // =============================================================================================================
        
        public void RegisterRegiment(ISelectableRegiment regiment)
        {
            HighlightSystems.ForEach(system => system.AddRegiment(regiment));
        }
        
        public void UnRegisterRegiment(ISelectableRegiment regiment)
        {
            HighlightSystems.ForEach(system => system.RemoveRegiment(regiment));
        }
    }
    */
}
