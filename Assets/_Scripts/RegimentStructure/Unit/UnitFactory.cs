using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Vector3;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;

namespace KaizerWald
{
    public class UnitFactory : MonoBehaviour
    {
        private const int RAYCAST_RANGE = 2000;
        private const int RAYCAST_UP_ORIGIN = 1000;
        
        [SerializeField] private LayerMask TerrainLayer;
        [SerializeField] private LayerMask UnitLayer;

        private int TerrainLayerIndex => floorlog2(TerrainLayer.value);
        private int UnitLayerIndex => floorlog2(UnitLayer.value);
        
        public List<Unit> CreateRegimentsUnit(Regiment regiment, int baseNumUnits, GameObject unitPrefab)
        {
            float3 regimentPosition = regiment.transform.position;
            List<Unit> units = new(baseNumUnits);
            
            for (int i = 0; i < baseNumUnits; i++)
            {
                //Vector2 positionInRegiment = GetPositionInRegiment(i, regimentPosition.xz, regiment.CurrentFormation);
                //Vector3 unitPosition = GetUnitPosition(positionInRegiment);
                Vector3 unitPosition = regiment.CurrentFormation.GetUnitRelativePositionToRegiment3D(i, regimentPosition);
                GameObject unitGameObject = Instantiate(unitPrefab, unitPosition, regiment.transform.localRotation);
                units.Add(InitializeUnitComponent(regiment, unitGameObject, i));
            }
            return units;
        }
        
        private Unit InitializeUnitComponent(Regiment regiment, GameObject unitGameObject, int index)
        {
            unitGameObject.name = $"{unitGameObject.name}_{index}";
            Unit component = unitGameObject.GetComponentInChildren<Unit>();
            if (component == null) Debug.Log("Dont have Component: Unit");
            for (int i = 0; i < unitGameObject.transform.childCount; i++)
            {
                Transform child = unitGameObject.transform.GetChild(i);
                if (!child.TryGetComponent(out Collider _)) continue;
                child.gameObject.AddComponent<Unit>();
                break;
            }
            return component.Initialize(regiment, UnitLayerIndex);
        }
        
        private Vector3 GetUnitPosition(in Vector2 positionInRegiment)
        {
            Vector3 position3D = new Vector3(positionInRegiment.x, 0, positionInRegiment.y);
            Vector3 origin = position3D + Vector3.up * RAYCAST_UP_ORIGIN;
            bool hasHit = Raycast(origin,Vector3.down, out RaycastHit hit, RAYCAST_RANGE, TerrainLayer.value);
            return hasHit ? hit.point : position3D;
        }
        
        private Vector2 GetPositionInRegiment(int index, in float2 regimentPosition, Formation formation)
        {
            //Coord according to index
            int y = index / formation.Width;
            int x = index - (y * formation.Width);
            float2 unitSize = formation.DistanceUnitToUnit;
            //Offset to place regiment in the center of the mass
            
            float offsetX = regimentPosition.x - GetOffset(formation.Width, unitSize.x);
            float offsetZ = regimentPosition.y - GetOffset(formation.Depth, unitSize.y);
            return new Vector2(x * unitSize.x + offsetX, -(y * unitSize.y) + offsetZ);
        }

        private float GetOffset(int row, float size)
        {
            float unitHalfOffset = size * 0.5f;
            float halfRow = row * 0.5f;
            return halfRow * size - unitHalfOffset;
        }
    }
    
}
