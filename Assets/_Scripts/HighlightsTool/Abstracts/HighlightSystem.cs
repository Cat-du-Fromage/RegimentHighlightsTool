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

        public virtual void AddRegiment(Regiment regiment)
        {
            Array.ForEach(Registers, register => register.RegisterRegiment(regiment));
        }

        public virtual void RemoveRegiment(Regiment regiment)
        {
            Array.ForEach(Registers, register => register.UnregisterRegiment(regiment));
        }
        
        public virtual void OnShow(Regiment regiment, int registerIndex)
        {
            if (regiment == null) return;
            if (!Registers[registerIndex].Records.TryGetValue(regiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                highlight.Show();
            }
            Registers[registerIndex].ActiveHighlights.Add(regiment);
        }
        
        public virtual void OnHide(Regiment regiment, int registerIndex)
        {
            if (regiment == null) return;
            if (!Registers[registerIndex].Records.TryGetValue(regiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                highlight.Hide();
            }
            Registers[registerIndex].ActiveHighlights.Remove(regiment);
        }

        public virtual void HideAll(int registerIndex)
        {
            for (int i = Registers[registerIndex].ActiveHighlights.Count - 1; i > -1; i--)
            {
                OnHide(Registers[registerIndex].ActiveHighlights[i], registerIndex);
            }
        }
    }
}