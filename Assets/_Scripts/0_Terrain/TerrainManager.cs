using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using KWUtils;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

using int2 = Unity.Mathematics.int2;

namespace KaizerWald
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class TerrainManager : MonoBehaviourSingleton<TerrainManager>
    {
        private enum ECardinal
        {
            North,
            South,
            East,
            West
        }
        
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        [SerializeField] private Material DefaultMaterial;
        [field: SerializeField] public int2 SizeXY { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [SerializeField] private GameObject PlayerOneSpawn;
        [SerializeField] private GameObject PlayerTwoSpawn;
        
        public GenericGrid<bool> ObstacleGrid { get; private set; }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected override void Awake()
        {
            base.Awake();
            CreateTerrain();
            InitializeSpawners();
            
            ObstacleGrid = new GenericGrid<bool>(SizeXY, 1);
        }

        private void OnDestroy()
        {
            ObstacleGrid?.Dispose();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void CreateTerrain()
        {
            transform.localScale = Vector3.one;
            // Initialization
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
            SizeXY = ceilpow2(max(SizeXY, 1));
            
            // Assignment
            Mesh terrainMesh = MeshUtils.CreatePlaneMesh(SizeXY);
            meshFilter.sharedMesh = terrainMesh;
            meshCollider.sharedMesh = terrainMesh;
            if (DefaultMaterial != null) meshRenderer.sharedMaterial = DefaultMaterial;
        }

        private void InitializeSpawners(int spawnerSize = 4, int borderOffset = 2)
        {
            if (PlayerOneSpawn == null) PlayerOneSpawn = transform.GetChild(0).gameObject;
            if (PlayerTwoSpawn == null) PlayerTwoSpawn = transform.GetChild(1).gameObject;

            PlayerOneSpawn.transform.position = GetSpawnerPosition(ECardinal.South, spawnerSize, borderOffset);
            PlayerOneSpawn.transform.localScale = new Vector3(0.1f * (SizeXY.x - borderOffset), 1, 0.1f * spawnerSize);
            
            PlayerTwoSpawn.transform.position = GetSpawnerPosition(ECardinal.North, spawnerSize, borderOffset);
            PlayerTwoSpawn.transform.localScale = new Vector3(0.1f * (SizeXY.x - borderOffset), 1, 0.1f * spawnerSize);
        }
        
        private Vector3 GetSpawnerPosition(ECardinal direction, int spawnerSizeY, int borderOffset)
        {
            Vector2 halfSizeXY = (float2)SizeXY / 2f;
            Vector3Int dirOffset = direction switch
            {
                ECardinal.North => Vector3Int.forward,
                ECardinal.South => Vector3Int.back,
                ECardinal.East  => Vector3Int.right,
                ECardinal.West  => Vector3Int.left,
                _ => Vector3Int.zero,
            };
            
            Vector3 offset = (Vector3)dirOffset * (dirOffset.x == 0 ? halfSizeXY.y : halfSizeXY.x);
            offset -= (Vector3)dirOffset * (spawnerSizeY / 2f + borderOffset);
            offset += Vector3.up * 0.01f;
            return offset;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Spawner ◈◈◈◈◈◈                                                                                      ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public Transform GetSpawnerTransform(int spawnIndex)
        {
            return spawnIndex == 0 ? PlayerOneSpawn.transform : PlayerTwoSpawn.transform;
        }

        public Vector3 GetPlayerFirstSpawnPosition(int spawnIndex)
        {
            if (spawnIndex is < 0 or > 1) return Vector3.zero;
            Transform spawnerTransform = spawnIndex == 0 ? PlayerOneSpawn.transform : PlayerTwoSpawn.transform;
            float spawnHorizontalSize = spawnerTransform.parent.localScale.x / 2f;
            Vector3 firstSpawnPoint = spawnerTransform.position - spawnerTransform.right * spawnHorizontalSize;
            return Vector3.Scale(firstSpawnPoint, new Vector3(1f,0,1f));
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Grid System ◈◈◈◈◈◈                                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    }
}
