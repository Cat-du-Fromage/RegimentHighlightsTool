using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class Unit : MonoBehaviour, IUnit
    {
        public SelectableRegiment SelectableRegimentAttach { get; set; }
        public int IndexInRegiment { get; set; } = 1;

        public bool IsDead { get; private set; }
    }
}