using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;

namespace KaizerWald
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class TerrainGenerator : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        [SerializeField] private Material DefaultMaterial;
        [field: SerializeField] public int2 SizeXY { get; private set; }

        private void Awake()
        {
            // Initialization
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
            SizeXY = max(SizeXY, int2(1));
            
            // Assignment
            Mesh terrainMesh = MeshUtils.CreatePlaneMesh(SizeXY);
            meshFilter.sharedMesh = terrainMesh;
            meshCollider.sharedMesh = terrainMesh;
            if (DefaultMaterial != null) meshRenderer.sharedMaterial = DefaultMaterial;
        }
    }
}
