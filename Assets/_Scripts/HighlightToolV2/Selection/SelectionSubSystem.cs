using UnityEngine;
using ISelectableRegiment = KaizerWald.ISelectableRegiment;

namespace KaizerWald2
{
    public sealed class SelectionSubSystem : HighlightSubSystem
    {
        public SelectionSubSystem(HighlightSystem mainSystem, GameObject defaultPrefab) : base(mainSystem, defaultPrefab)
        {
            Register = new SelectionRegister(mainSystem, defaultPrefab);
        }
        
        public override void OnShow(ISelectableRegiment selectableRegiment)
        {
            base.OnShow(selectableRegiment);
            selectableRegiment.IsSelected = true;
            Register.ActiveHighlights.Add(selectableRegiment);
        }

        public override void OnHide(ISelectableRegiment selectableRegiment)
        {
            base.OnHide(selectableRegiment);
            selectableRegiment.IsSelected = false;
            Register.ActiveHighlights.Remove(selectableRegiment);
        }

        public override void HideAll()
        {
            base.HideAll();
            Register.ActiveHighlights.ForEach(regiment => { regiment.IsSelected = false; });
            Register.ActiveHighlights.Clear();
        }
    }
}