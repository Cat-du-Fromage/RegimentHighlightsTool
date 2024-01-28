#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaizerwald
{
    public partial class ProjectileManager : MonoBehaviour
    {
        private void OnGUI()
        {
            DebugActiveCount();
        }
        
        private void DebugActiveCount()
        {
            if (!Keyboard.current.tKey.wasPressedThisFrame) return;
            int numBullet = FindObjectsByType<ProjectileComponent>(FindObjectsSortMode.None).Length;
            Debug.Log($"num Bullet On Game: {numBullet}; activeRegister: {ActiveBullets.Count}");
        }
    }
}
#endif