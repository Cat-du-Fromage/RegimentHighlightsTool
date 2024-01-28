using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

using static Kaizerwald.KzwMath;
using static Kaizerwald.UnityMathematicsUtilities;

namespace Kaizerwald
{
    public class TerrainGrid : MonoBehaviourSingleton<TerrainGrid>
    {
        [SerializeField] private TerrainManager Terrain;

        public int CellSize;
        

        protected override void Awake()
        {
            base.Awake();
            Terrain = FindObjectOfType<TerrainManager>();
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            int numCells = cmul(Terrain.SizeXY);
            for (int i = 0; i < numCells; i++)
            {
                (int x, int y) = GetXY(i, Terrain.SizeXY.x);
                
            }
        }
    }
}
