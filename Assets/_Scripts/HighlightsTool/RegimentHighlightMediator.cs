using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace KaizerWald
{
    public class RegimentHighlightMediator : HighlightMediator
    {
        private PlayerControls HighlightControls;
        
        [Header("Default Prefabs")]
        [SerializeField] private GameObject PreselectionDefaultPrefab;
        [SerializeField] private GameObject SelectionDefaultPrefab;

        private RegimentFactory factory;

        private void Awake()
        {
            HighlightControls = new PlayerControls();
            factory = FindObjectOfType<RegimentFactory>();
            HighlightSystems = new List<HighlightSystem>()
            {
                new RegimentSelection(this, HighlightControls, UnitLayerMask, PreselectionDefaultPrefab, SelectionDefaultPrefab),
            };
        }
        
        private void Start()
        {
            //yield return new WaitUntil(() => factory.CreationOrders.Length == 0);
            GetBasedRegiments();
        }

        private void GetBasedRegiments()
        {
            List<ISelectableRegiment> selectables = GameObjectExtension.FindObjectsOfInterface<ISelectableRegiment>();
            Regiments = new List<ISelectableRegiment>(selectables);
            foreach (ISelectableRegiment regiment in selectables)
            {
                Debug.Log($"num Units: {regiment.UnitsTransform.Length}");
                if (regiment.OwnerID != PlayerID) continue;
                Regiments.Add(regiment);
                RegisterRegiment(regiment);
            }
        }
    }
}
