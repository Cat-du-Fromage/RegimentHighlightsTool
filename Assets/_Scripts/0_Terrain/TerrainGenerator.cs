using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace KaizerWald
{
    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField] private Material DefaultMaterial;
        [field: SerializeField] public int NumQuadPerLine { get; private set; }
        [field: SerializeField] public int2 NumChunkXY { get; private set; }
        //[field: SerializeField] public NoiseData NoiseSettings { get; private set; }
        //[field: SerializeField] public TerrainSettings Settings { get; private set; }
        
        
    }
}
