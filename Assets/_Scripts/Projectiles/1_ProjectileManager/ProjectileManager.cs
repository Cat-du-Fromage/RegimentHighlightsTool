using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWald
{
    public partial class ProjectileManager : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                          ◆◆◆◆◆◆ STATIC PROPERTIES ◆◆◆◆◆◆                                           ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public static ProjectileManager Instance { get; private set; }
        private bool initialized;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //TODO MAKE POOL BY REGIMENT!!!! so we can destroy them
        private Dictionary<int, ObjectPool<ProjectileComponent>> RegimentBulletsPool = new();
        
        private List<ProjectileComponent> ActiveBullets = new(10);
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void Awake()
        {
            InitializeSingleton();
        }

        private void Start()
        {
            RegisterEvent();
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                    ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void Update()
        {
            if (ActiveBullets.Count == 0) return;
            foreach (ProjectileComponent activeBullet in ActiveBullets)
            {
                activeBullet.OnUpdate();
            }
        }

        //A SURVEILLER DE PRES: POSSIBLE SOURCE DE BUG!
        private void LateUpdate()
        {
            if (ActiveBullets.Count == 0) return;
            CleanActiveBullets();
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Enable | Disable ◈◈◈◈◈◈                                                                        ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void OnEnable()
        {
            RegisterEvent();
        }

        private void OnDisable()
        {
            if (HighlightRegimentManager.Instance == null) return;
            RegimentManager.Instance.OnNewRegiment -= RegisterPool;
        }

        private void RegisterEvent()
        {
            if (HighlightRegimentManager.Instance == null || initialized) return;
            RegimentManager.Instance.OnNewRegiment += RegisterPool;
            initialized = true;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Object Pooling Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public ProjectileComponent UnitRequestAmmo(Unit unit, Vector3 positionInRifle)
        {
            int regimentID = unit.RegimentAttach.RegimentID;
            return !RegimentBulletsPool.TryGetValue(regimentID, out ObjectPool<ProjectileComponent> pool) ? null : pool.Pull(positionInRifle);
        }

        private void CleanActiveBullets()
        {
            for (int i = ActiveBullets.Count - 1; i > -1; i--)
            {
                if (ActiveBullets[i].isActiveAndEnabled) continue;
                ActiveBullets.RemoveAt(i);
            }
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Register / Unregister ◈◈◈◈◈◈                                                                   ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void RegisterPool(Regiment regiment)
        {
            GameObject prefab = regiment.RegimentType.BulletPrefab;
            int unitCount = regiment.CurrentFormation.BaseNumUnits;
            RegimentBulletsPool.TryAdd(regiment.RegimentID, new ObjectPool<ProjectileComponent>(prefab, CallOnPull, unitCount));
        }
        
        private void CallOnPull(ProjectileComponent projectile)
        {
            ActiveBullets.Add(projectile);
        }
        //private void CallOnPush(ProjectileComponent projectile) { }
    }
}
