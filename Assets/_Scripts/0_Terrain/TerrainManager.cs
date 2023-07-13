using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KaizerWald
{
    public class TerrainManager : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [SerializeField] private GameObject PlayerOneSpawn;
        [SerializeField] private GameObject PlayerTwoSpawn;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public static TerrainManager Instance { get; private set; }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void Awake()
        {
            InitializeSingleton();
            if (PlayerOneSpawn == null) PlayerOneSpawn = transform.GetChild(0).gameObject;
            if (PlayerTwoSpawn == null) PlayerTwoSpawn = transform.GetChild(1).gameObject;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Transform GetSpawnerTransform(int spawnIndex)
        {
            return spawnIndex is 0 ? PlayerOneSpawn.transform : PlayerTwoSpawn.transform;
        }

        public Vector3 GetPlayerFirstSpawnPosition(int spawnIndex)
        {
            if (spawnIndex is < 0 or > 1) return Vector3.zero;
            Transform spawnerTransform = spawnIndex is 0 ? PlayerOneSpawn.transform : PlayerTwoSpawn.transform;
            Vector3 spawnerCenter = spawnerTransform.position;
            float spawnHorizontalSize = spawnerTransform.parent.localScale.x;
            Vector3 firstSpawnPoint = spawnerCenter - spawnerTransform.right * (spawnHorizontalSize / 2);
            return firstSpawnPoint;
        }

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
        
        
    }
}
