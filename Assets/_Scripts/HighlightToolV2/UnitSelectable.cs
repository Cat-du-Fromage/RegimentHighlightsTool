using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISelectableRegiment = KaizerWald.ISelectableRegiment;

namespace KaizerWald
{
    public class UnitSelectable : MonoBehaviour
    {
        public ISelectableRegiment SelectableRegimentAttach { get; private set; }

        public void SetRegiment(ISelectableRegiment regiment) => SelectableRegimentAttach = regiment;
    }
}
