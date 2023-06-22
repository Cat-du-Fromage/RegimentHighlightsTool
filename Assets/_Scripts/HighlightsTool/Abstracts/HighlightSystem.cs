using ISelectableRegiment = KaizerWald.ISelectableRegiment;

namespace KaizerWald
{
    /// <summary>
    /// HIGHLIGHT SYSTEM
    /// </summary>
    public abstract class HighlightSystem
    {
        public abstract void AddRegiment(ISelectableRegiment regiment);
        public abstract void RemoveRegiment(ISelectableRegiment regiment);
    }

    /*
    public abstract class SimpleSystem : HighlightSystem
    {
        public HighlightCoordinator Coordinator { get; protected set; }
        public HighlightRegister Register { get; protected set; }
        public HighlightController Controller { get; protected set; }

        protected SimpleSystem(HighlightCoordinator coordinator, GameObject defaultPrefab)
        {
            Coordinator = coordinator;
        }

        public sealed override void AddRegiment(ISelectableRegiment regiment) => Register.RegisterRegiment(regiment);
        public sealed override void RemoveRegiment(ISelectableRegiment regiment) => Register.UnregisterRegiment(regiment);

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
    */
}