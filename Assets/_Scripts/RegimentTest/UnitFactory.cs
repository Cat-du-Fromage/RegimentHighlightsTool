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
        [SerializeField] private LayerMask PlayerLayer;
        [SerializeField] private LayerMask EnemyLayer;
        public List<Transform> CreateRegimentsUnit(Regiment regiment, int baseNumUnits, GameObject unitPrefab)
        {
            Vector3 regimentPosition = regiment.transform.position;

            float unitSizeX = 1.5f;//regimentClass.UnitSize.x;

            List<Transform> units = new(baseNumUnits);
            
            for (int i = 0; i < baseNumUnits; i++)
            {
                int terrainLayer = floorlog2(TerrainLayer.value);
                Vector3 positionInRegiment = GetPositionInRegiment(i);

                Vector3 origin = positionInRegiment + Vector3.up * 1000;
                bool hasHit = Raycast(origin, Vector3.down, out RaycastHit hit, 2000, 1<<terrainLayer);
                
                Vector3 unitPosition = hasHit ? hit.point : positionInRegiment;
                    
                GameObject newUnit = Instantiate(unitPrefab, unitPosition, regiment.transform.rotation);
                newUnit.name = $"{unitPrefab.name}_{i}";

                units.Add(InitializeUnitComponent(newUnit, i).transform);
            }
            return units;

            Unit InitializeUnitComponent(GameObject unit, int index)
            {
                Unit component = unit.GetComponentInChildren<Unit>();
                if (component == null) Debug.Log("Dont have Component: Unit");
                //Unit component = unit.GetComponent<Unit>();
                component.SelectableRegimentAttach = regiment.GetComponent<SelectableRegiment>();
                component.IndexInRegiment = index;
                return component;
            }

            //Internal Methods
            // -------------------------------------------------------------------------
            Vector3 GetPositionInRegiment(int index)
            {
                int row = 10;
                int column = 2;
                
                //Coord according to index
                int y = index / row;
                int x = index - (y * row);

                //Offset to place regiment in the center of the mass
                float offsetX = regimentPosition.x - GetOffset(row);
                float offsetZ = regimentPosition.z - GetOffset(column);
                return new Vector3(x * unitSizeX + offsetX, 0, -(y * unitSizeX) + offsetZ);
            }

            float GetOffset(int row)
            {
                float unitHalfOffset = unitSizeX / 2f;
                float halfRow = row / 2f;
                return halfRow * unitSizeX - unitHalfOffset;
            }
        }
    }
    
}
