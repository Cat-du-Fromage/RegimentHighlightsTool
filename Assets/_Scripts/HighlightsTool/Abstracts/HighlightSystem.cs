using System;
using UnityEngine;

namespace KaizerWald
{
    /// <summary>
    /// HIGHLIGHT SYSTEM
    /// </summary>
    public abstract class HighlightSystem : MonoBehaviour
    {
        protected HighlightRegister[] Registers = new HighlightRegister[2];
        public HighlightController Controller { get; protected set; }

        public virtual void AddRegiment(SelectableRegiment regiment)
        {
            Array.ForEach(Registers, register => register.RegisterRegiment(regiment));
        }

        public virtual void RemoveRegiment(SelectableRegiment regiment)
        {
            Array.ForEach(Registers, register => register.UnregisterRegiment(regiment));
        }
        
        public virtual void OnShow(SelectableRegiment selectableRegiment, int registerIndex)
        {
            if (selectableRegiment == null) return;
            if (!Registers[registerIndex].Records.TryGetValue(selectableRegiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                highlight.Show();
            }
        }
        
        public virtual void OnHide(SelectableRegiment selectableRegiment, int registerIndex)
        {
            if (selectableRegiment == null) return;
            if (!Registers[registerIndex].Records.TryGetValue(selectableRegiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                highlight.Hide();
            }
        }
        
        public virtual void HideAll(int registerIndex)
        {
            foreach (SelectableRegiment activeHighlight in Registers[registerIndex].ActiveHighlights)
            {
                foreach (HighlightBehaviour highlight in Registers[registerIndex].Records[activeHighlight.RegimentID])
                {
                    if (highlight == null) continue;
                    highlight.Hide();
                }
            }
        }
    }
}