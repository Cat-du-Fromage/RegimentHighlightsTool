using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils
{
    [Serializable]
    public class TerrainSettings
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [field: SerializeField] public int NumQuadPerLine { get; private set; }
        [field: SerializeField] public int NumChunkWidth { get; private set; }
        [field: SerializeField] public int NumChunkHeight { get; private set; }
        [field: SerializeField] public NoiseData Noise { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                   ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public int2 NumChunkAxis => new int2(NumChunkWidth, NumChunkHeight);
        public int ChunksCount => NumChunkWidth * NumChunkHeight;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ QUADS ◇◇◇◇◇◇                                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public int QuadCount => NumQuadX * NumQuadY;
        public int2 NumQuadsAxis => new int2(NumQuadX, NumQuadY);
        public int NumQuadX => NumChunkWidth * NumQuadPerLine;
        public int NumQuadY => NumChunkHeight * NumQuadPerLine;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ VERTEX ◇◇◇◇◇◇                                                                                      │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public int MapVerticesCount => NumVerticesX * NumVerticesY;
        public int2 NumVerticesAxis => new int2(NumVerticesX, NumVerticesY);
        public int NumVerticesX => NumQuadX + 1;
        public int NumVerticesY => NumQuadY + 1;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ CHUNK DIRECT ACCESS ◈◈◈◈◈◈                                                                         ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ QUADS ◇◇◇◇◇◇                                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public int ChunkQuadsPerLine => NumQuadPerLine;
        public int ChunkQuadsCount   => NumQuadPerLine * NumQuadPerLine;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ VERTEX ◇◇◇◇◇◇                                                                                      │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public int ChunkVerticesPerLine => NumQuadPerLine + 1;
        public int ChunkVerticesCount   => ChunkVerticesPerLine * ChunkVerticesPerLine;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ TRIANGLES ◇◇◇◇◇◇                                                                                   │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public int ChunkTrianglesCount => (NumQuadPerLine * NumQuadPerLine) * 2;
        public int ChunkIndicesCount   => ChunkQuadsCount * 6;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public TerrainSettings(int numQuadPerLine, int2 numChunksXY, NoiseData noise)
        {
            NumQuadPerLine = max(1,ceilpow2(clamp(numQuadPerLine, 1, 64)));
            NumChunkWidth  = max(1, ceilpow2(numChunksXY.x));
            NumChunkHeight = max(1, ceilpow2(numChunksXY.y));
            Noise = new NoiseData(noise);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ OPERATORS ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        //public static implicit operator ChunkData(TerrainSettings settings) => new ChunkData(settings);
        public static implicit operator MapData(TerrainSettings settings) => new MapData(settings);
        public static implicit operator NoiseData(TerrainSettings settings) => new NoiseData(settings.Noise);
    }
}
