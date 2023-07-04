using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Physics;
using static Unity.Mathematics.math;

namespace KaizerWald
{
    public class UnitFactory : MonoBehaviour
    {
        [SerializeField] private LayerMask TerrainLayer;
        [SerializeField] private LayerMask UnitLayer;

        private int TerrainLayerIndex => floorlog2(TerrainLayer.value);
        private int UnitLayerIndex => floorlog2(UnitLayer.value);
        
        public List<Unit> CreateRegimentsUnit(Regiment regiment, int baseNumUnits, GameObject unitPrefab)
        {
            Vector3 regimentPosition = regiment.transform.position;
            float2 unitSize = regiment.Formation.DistanceUnitToUnit; // 1.5f
            List<Unit> units = new(baseNumUnits);
            
            for (int i = 0; i < baseNumUnits; i++)
            {
                Vector3 positionInRegiment = GetPositionInRegiment(i, regiment.Formation);
                Vector3 unitPosition = GetUnitPosition(positionInRegiment);
                
                GameObject newUnit = Instantiate(unitPrefab, unitPosition, regiment.transform.rotation);
                newUnit.name = $"{unitPrefab.name}_{i}";

                units.Add(InitializeUnitComponent(newUnit, i));
            }
            return units;

            Vector3 GetUnitPosition(Vector3 positionInRegiment)
            {
                Vector3 origin = positionInRegiment + Vector3.up * 1000;
                bool hasHit = Raycast(origin, Vector3.down, out RaycastHit hit, 2000, TerrainLayer);
                return hasHit ? hit.point : positionInRegiment;
            }

            Unit InitializeUnitComponent(GameObject unit, int index)
            {
                Unit component = unit.GetComponentInChildren<Unit>();
                component.gameObject.layer = UnitLayerIndex;
                
                if (component == null) Debug.Log("Dont have Component: Unit");
                component.IndexInRegiment = index;
                return component;
            }

            //Internal Methods
            // -------------------------------------------------------------------------
            Vector3 GetPositionInRegiment(int index, FormationData formation)
            {
                //Coord according to index
                int y = index / formation.Width;
                int x = index - (y * formation.Width);

                //Offset to place regiment in the center of the mass
                float offsetX = regimentPosition.x - GetOffset(formation.Width, unitSize.x);
                float offsetZ = regimentPosition.z - GetOffset(formation.Depth, unitSize.y);
                return new Vector3(x * unitSize.x + offsetX, 0, -(y * unitSize.y) + offsetZ);
            }

            float GetOffset(int row, float size)
            {
                float unitHalfOffset = size * 0.5f;
                float halfRow = row * 0.5f;
                return halfRow * size - unitHalfOffset;
            }
        }
    }
    
}
