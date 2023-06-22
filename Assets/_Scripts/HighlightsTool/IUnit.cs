using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public interface IUnit
    {
        public ISelectableRegiment SelectableRegimentAttach { get; }
    }
}