using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISelectableRegiment = KaizerWald.ISelectableRegiment;

namespace KaizerWald
{
    public abstract class HighlightRegister
    {
        protected HighlightSystem System { get; private set; }
        protected GameObject Prefab { get; private set; }
        public Dictionary<int, HighlightBehaviour[]> Records { get; protected set; }
        public List<ISelectableRegiment> ActiveHighlights { get; protected set; }

        protected HighlightRegister(HighlightSystem system, GameObject highlightPrefab)
        {
            if (!highlightPrefab.TryGetComponent<HighlightBehaviour>(out _))
            {
                Debug.LogError("Prefab Don't have component: HighlightBehaviour");
            }
            System = system;
            Prefab = highlightPrefab;
            Records = new Dictionary<int, HighlightBehaviour[]>();
            ActiveHighlights = new List<ISelectableRegiment>();
        }
        
        private void PopulateRecords(ISelectableRegiment selectableRegiment, GameObject prefab)
        {
            Records[selectableRegiment.RegimentID] ??= new HighlightBehaviour[selectableRegiment.UnitsTransform.Length];
            for (int i = 0; i < Records[selectableRegiment.RegimentID].Length; i++)
            {
                GameObject highlightObj = Object.Instantiate(prefab);
                Records[selectableRegiment.RegimentID][i] = highlightObj.GetComponent<HighlightBehaviour>();
                Records[selectableRegiment.RegimentID][i].InitializeHighlight(selectableRegiment.UnitsTransform[i]);
            }
        }

        public void RegisterRegiment(ISelectableRegiment selectableRegiment, GameObject prefabOverride = null)
        {
            GameObject highlightPrefab = prefabOverride == null ? Prefab : prefabOverride;
            Records.TryAdd(selectableRegiment.RegimentID, new HighlightBehaviour[selectableRegiment.UnitsTransform.Length]);
            PopulateRecords(selectableRegiment, highlightPrefab);
        }
        
        public void UnregisterRegiment(ISelectableRegiment selectableRegiment)
        {
            if (!Records.TryGetValue(selectableRegiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                Object.Destroy(highlight);
            }
            Records.Remove(selectableRegiment.RegimentID);
        }
    }
}
