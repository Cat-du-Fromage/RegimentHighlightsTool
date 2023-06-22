using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISelectableRegiment = KaizerWald.ISelectableRegiment;
namespace KaizerWald2
{
    public class UnitSelectable : MonoBehaviour
    {
        public ISelectableRegiment SelectableRegimentAttach { get; private set; }

        public void SetRegiment(ISelectableRegiment regiment) => SelectableRegimentAttach = regiment;
    }
}
