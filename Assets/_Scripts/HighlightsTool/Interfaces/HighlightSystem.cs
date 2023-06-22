

using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace KaizerWald
{
    public abstract class HighlightSystem
    {
        public HighlightCoordinator Coordinator { get; protected set; }
        public HighlightRegister Register { get; protected set; }
        public HighlightController Controller { get; protected set; }

        public event Action OnControllerEvent;

        protected HighlightSystem(HighlightCoordinator coordinator, GameObject defaultPrefab)
        {
            if (defaultPrefab == null)
            {
                Debug.LogError("Prefab Reference Null");
            }
            Coordinator = coordinator;
        }

        public void AddRegiment(ISelectableRegiment regiment) => Register.RegisterRegiment(regiment);
        public void RemoveRegiment(ISelectableRegiment regiment) => Register.UnregisterRegiment(regiment);

        public virtual void Notify()
        {
            OnControllerEvent?.Invoke();
        }

        // =============================================================================================================
        // ----- VIRTUAL METHODS -----
        // =============================================================================================================
        
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