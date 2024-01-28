using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public class HighlightUnit : MonoBehaviour
    {
        [field: SerializeField] public bool IsActive { get; private set; } = true;
        [field: SerializeField] public HighlightRegiment RegimentAttach { get; private set; }

        private void OnDestroy()
        {
            RegimentAttach.RemoveUnit(this);
        }
        
        public void AttachToRegiment(HighlightRegiment regimentToAttach) => RegimentAttach = regimentToAttach;
        public void SetActive() => IsActive = true;
        public void SetInactive() => IsActive = false;
    }
}
