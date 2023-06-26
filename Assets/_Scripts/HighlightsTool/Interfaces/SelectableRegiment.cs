using System;
using UnityEngine;

namespace KaizerWald
{
    /// <summary>
    /// REFACTOR: Make it monobehaviour component
    /// </summary>
    public class SelectableRegiment : MonoBehaviour
    {
        public ulong OwnerID { get; }
        public int RegimentID { get; }
        public bool IsPreselected { get; set; }
        public bool IsSelected { get; set; }
        public Transform[] UnitsTransform { get; set; }

        public void SetSelectableProperties(ESelection index, bool value)
        {
            switch (index)
            {
                case ESelection.Preselection:
                    IsPreselected = value;
                    return;
                case ESelection.Selection:
                    IsSelected = value;
                    return;
                default:
                    return;
            }
        }
    }
}