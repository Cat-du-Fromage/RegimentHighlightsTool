using UnityEngine;
using ISelectableRegiment = KaizerWald.ISelectableRegiment;

namespace KaizerWald
{
    public abstract class HighlightSubSystem : HighlightSystem
    {
        public HighlightRegister Register { get; protected set; }
        
        protected HighlightSubSystem(HighlightSystem mainSystem, GameObject defaultPrefab)
        {
            if (defaultPrefab == null) Debug.LogError("Prefab Reference Null");
        }
        
        public override void AddRegiment(ISelectableRegiment regiment) => Register.RegisterRegiment(regiment);
        public override void RemoveRegiment(ISelectableRegiment regiment) => Register.UnregisterRegiment(regiment);
        
        /// <summary>
        /// Show Highlight
        /// </summary>
        /// <param name="selectableRegiment"></param>
        public virtual void OnShow(ISelectableRegiment selectableRegiment)
        {
            if (selectableRegiment == null) return;
            if (!Register.Records.TryGetValue(selectableRegiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                highlight.Show();
            }
        }

        /// <summary>
        /// Hide Highlight
        /// </summary>
        /// <param name="selectableRegiment"></param>
        public virtual void OnHide(ISelectableRegiment selectableRegiment)
        {
            if (selectableRegiment == null) return;
            if (!Register.Records.TryGetValue(selectableRegiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                highlight.Hide();
            }
        }

        /// <summary>
        /// Hide all highlight
        /// </summary>
        public virtual void HideAll()
        {
            foreach (ISelectableRegiment activeHighlight in Register.ActiveHighlights)
            {
                foreach (HighlightBehaviour highlight in Register.Records[activeHighlight.RegimentID])
                {
                    if (highlight == null) continue;
                    highlight.Hide();
                }
            }
        }
    }
}