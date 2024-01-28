#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaizerwald
{
    public partial class HighlightRegimentManager : MonoBehaviourSingleton<HighlightRegimentManager>
    {
        private void OnGUI()
        {
            TestKillUnit();
        }

        private void TestKillUnit()
        {
            if (!Mouse.current.rightButton.wasReleasedThisFrame) return;
            Ray singleRay = Camera.main.ScreenPointToRay(Mouse.current.position.value);
            if (!Physics.Raycast(singleRay, out RaycastHit hit, 1000, 1 << 7)) return;
            Unit unit = hit.transform.GetComponent<Unit>();
            unit.TriggerDeath();
        }
    }
}

#endif
