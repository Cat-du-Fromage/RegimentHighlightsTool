using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    /// <summary>
    /// REFACTOR: Make it monobehaviour component
    /// </summary>
    public interface IUnit
    {
        public SelectableRegiment SelectableRegimentAttach { get; }
    }
}