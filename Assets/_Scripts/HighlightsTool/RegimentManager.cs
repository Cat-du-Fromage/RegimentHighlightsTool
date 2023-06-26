using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;

namespace KaizerWald
{
    public class RegimentManager : HighlightCoordinator
    {
        private List<Regiment> Regiments;
        private RegimentFactory factory;

        private void Awake()
        {
            HighlightControls = new PlayerControls();
            GameObject[] prefabs = new[] { PreselectionDefaultPrefab, SelectionDefaultPrefab };
            //RegimentHighlightSystem = new RegimentHighlightSystem(this, prefabs, HighlightControls, UnitLayerMask);
            factory = FindObjectOfType<RegimentFactory>();
        }
        
        private void Start()
        {
            //yield return new WaitUntil(() => factory.CreationOrders.Length == 0);
            Regiments = FindObjectsByType<Regiment>(FindObjectsSortMode.None).ToList();
            GetBasedRegiments();
        }

        private void GetBasedRegiments()
        {
            List<SelectableRegiment> selectables = GameObjectExtension.FindObjectsOfInterface<SelectableRegiment>();
            SelectableRegiments = new List<SelectableRegiment>(selectables);
            foreach (SelectableRegiment regiment in selectables)
            {
                Debug.Log($"num Units: {regiment.UnitsTransform.Length}");
                if (regiment.OwnerID != PlayerID) continue;
                SelectableRegiments.Add(regiment);
                RegisterRegiment(regiment);
            }
        }
    }
}
