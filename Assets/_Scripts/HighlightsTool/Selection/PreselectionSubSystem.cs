using UnityEngine;

namespace KaizerWald
{
    public sealed class PreselectionSubSystem : HighlightSubSystem
    {
        public PreselectionSubSystem(HighlightSystem mainSystem, GameObject defaultPrefab) : base(mainSystem, defaultPrefab)
        {
            Register = new HighlightRegister(mainSystem, defaultPrefab);
        }

        public override void OnShow(ISelectableRegiment selectableRegiment)
        {
            base.OnShow(selectableRegiment);
            selectableRegiment.IsPreselected = true;
            Register.ActiveHighlights.Add(selectableRegiment);
        }

        public override void OnHide(ISelectableRegiment selectableRegiment)
        {
            base.OnHide(selectableRegiment);
            selectableRegiment.IsPreselected = false;
            Register.ActiveHighlights.Remove(selectableRegiment);
        }

        public override void HideAll()
        {
            base.HideAll();
            Register.ActiveHighlights.ForEach(regiment => { regiment.IsPreselected = false; });
            Register.ActiveHighlights.Clear();
        }
    }
}