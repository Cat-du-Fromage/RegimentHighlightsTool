using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KaizerWald
{
    public class HighlightRegister
    {
        protected HighlightSystem System { get; private set; }
        protected GameObject Prefab { get; private set; }
        public Dictionary<int, HighlightBehaviour[]> Records { get; protected set; }
        public List<Regiment> ActiveHighlights { get; protected set; }

        public HighlightRegister(HighlightSystem system, GameObject highlightPrefab)
        {
            if (!highlightPrefab.TryGetComponent<HighlightBehaviour>(out _))
            {
                Debug.LogError("Prefab Don't have component: HighlightBehaviour");
            }
            System = system;
            Prefab = highlightPrefab;
            Records = new Dictionary<int, HighlightBehaviour[]>();
            ActiveHighlights = new List<Regiment>();
        }
        
        public HighlightBehaviour[] this[int regimentIndex] => Records[regimentIndex];

        private void PopulateRecords(Regiment selectableRegiment, GameObject prefab)
        {
            Records[selectableRegiment.RegimentID] ??= new HighlightBehaviour[selectableRegiment.UnitsTransform.Count];
            for (int i = 0; i < Records[selectableRegiment.RegimentID].Length; i++)
            {
                GameObject highlightObj = Object.Instantiate(prefab);
                Records[selectableRegiment.RegimentID][i] = highlightObj.GetComponent<HighlightBehaviour>();
                Records[selectableRegiment.RegimentID][i].InitializeHighlight(selectableRegiment.Units[i]);
            }
        }

        public void RegisterRegiment(Regiment selectableRegiment, GameObject prefabOverride = null)
        {
            GameObject highlightPrefab = prefabOverride == null ? Prefab : prefabOverride;
            Records.TryAdd(selectableRegiment.RegimentID, new HighlightBehaviour[selectableRegiment.UnitsTransform.Count]);
            PopulateRecords(selectableRegiment, highlightPrefab);
        }
        
        public void UnregisterRegiment(Regiment selectableRegiment)
        {
            if (!Records.TryGetValue(selectableRegiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                Object.Destroy(highlight);
            }
            Records.Remove(selectableRegiment.RegimentID);
        }
        
        
        public virtual void ResizeBuffer(int regimentID, int numDead)
        {
            if (!Records.ContainsKey(regimentID) || Records[regimentID].Length == 0) return;
            bool isRegimentWipedOut = numDead >= Records[regimentID].Length;
            if (isRegimentWipedOut)
            {
                WipeOutRegiment(regimentID);
            }
            else
            {
                CleanUnusedHighlights(regimentID, numDead);
            }
        }

        private void CleanUnusedHighlights(int regimentID, int numDead)
        {
            List<HighlightBehaviour> highlightsToDestroy = new (numDead);
            List<HighlightBehaviour> highlightsKept = new (Records[regimentID].Length - numDead);
            foreach (HighlightBehaviour highlight in Records[regimentID])
            {
                Unit unit = highlight.UnitAttach;
                if (unit.IsDead)
                {
                    highlightsToDestroy.Add(highlight);
                }
                else
                {
                    highlightsKept.Add(highlight);
                }
            }
            
            for (int i = highlightsToDestroy.Count - 1; i > -1; i--)
            {
                Object.Destroy(highlightsToDestroy[i]);
            }
            Records[regimentID] = highlightsKept.ToArray();
        }

        private void WipeOutRegiment(int regimentID)
        {
            for (int i = Records[regimentID].Length - 1; i > -1; i--) //try foreach after
            {
                Object.Destroy(Records[regimentID][i]);
            }
            Records[regimentID] = Array.Empty<HighlightBehaviour>();
        }
    }
}
