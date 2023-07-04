using System;
using UnityEngine;

namespace KaizerWald
{
    /// <summary>
    /// HIGHLIGHT SYSTEM
    /// </summary>
    public abstract class HighlightSystem : MonoBehaviour
    {
        protected HighlightCoordinator Coordinator;
        protected RegimentHighlightSystem MainSystem { get; private set; }
        
        protected HighlightRegister[] Registers = new HighlightRegister[2];
        public HighlightController Controller { get; protected set; }

        protected virtual void Awake()
        {
            MainSystem = GetComponent<RegimentHighlightSystem>();
            Coordinator = FindFirstObjectByType<HighlightCoordinator>();
            InitializeController();
            InitializeRegisters();
        }

        protected abstract void InitializeController();

        protected abstract void InitializeRegisters();
        /*
        protected void InitializeRegisters(GameObject[] prefabs)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                Registers[i] = new HighlightRegister(this, prefabs[i]);
            }
        }
*/
        public virtual void AddRegiment(Regiment regiment)
        {
            Array.ForEach(Registers, register => register.RegisterRegiment(regiment));
        }

        public virtual void RemoveRegiment(Regiment regiment)
        {
            Array.ForEach(Registers, register => register.UnregisterRegiment(regiment));
        }
        
        public virtual void OnShow(Regiment selectableRegiment, int registerIndex)
        {
            if (selectableRegiment == null) return;
            if (!Registers[registerIndex].Records.TryGetValue(selectableRegiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                highlight.Show();
            }
            Registers[registerIndex].ActiveHighlights.Add(selectableRegiment);
        }
        
        public virtual void OnHide(Regiment selectableRegiment, int registerIndex)
        {
            if (selectableRegiment == null) return;
            if (!Registers[registerIndex].Records.TryGetValue(selectableRegiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                highlight.Hide();
            }
            Registers[registerIndex].ActiveHighlights.Remove(selectableRegiment);
        }

        public virtual void HideAll(int registerIndex)
        {
            foreach (Regiment activeHighlight in Registers[registerIndex].ActiveHighlights)
            {
                foreach (HighlightBehaviour highlight in Registers[registerIndex].Records[activeHighlight.RegimentID])
                {
                    if (highlight == null) continue;
                    highlight.Hide();
                }
            }
            Registers[registerIndex].ActiveHighlights.Clear();
        }
    }
}