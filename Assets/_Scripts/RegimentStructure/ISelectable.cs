using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public interface ISelectable
    {
        public bool IsPreselected { get; set; }
        public bool IsSelected { get; set; }
        
        public void SetSelectableProperty(int index, bool value)
        {
            if (index is < 0 or > 1) return;
            if (index is 0)
                IsPreselected = value;
            else
                IsSelected = value;
        }
    }
}
