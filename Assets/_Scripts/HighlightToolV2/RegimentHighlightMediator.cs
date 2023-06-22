using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RegimentFactory = KaizerWald.RegimentFactory;
using ISelectableRegiment = KaizerWald.ISelectableRegiment;
using Object = UnityEngine.Object;

namespace KaizerWald2
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
                new RegimentSelection(this, HighlightControls, PlayerUnitLayerMask, PreselectionDefaultPrefab, SelectionDefaultPrefab),
            };
        }
        
        private /*IEnumerator*/void Start()
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
                foreach (Transform unit in regiment.UnitsTransform)
                {
                    Debug.Log($"unit: {unit.name}");
                    unit.gameObject.AddComponent<UnitSelectable>();
                }
                
                //Array.ForEach(regiment.UnitsTransform, unit => unit.gameObject.AddComponent<UnitSelectable>());
                if (regiment.OwnerID != PlayerID) continue;
                Regiments.Add(regiment);
                RegisterRegiment(regiment);
            }
        }
    }
}
