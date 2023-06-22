using System;
using System.Collections;
using System.Collections.Generic;
//using KWUtils;
using UnityEngine;
using Unity.Mathematics;

namespace KaizerWald
{
    [Serializable]
    public struct RegimentSpawner
    {
        public ulong OwnerID;
        public int Number;
        public int BaseNumUnit;
        public GameObject UnitPrefab;
    }

    [RequireComponent(typeof(UnitFactory))]
    public class RegimentFactory : MonoBehaviour
    {
        public static RegimentFactory Instance { get; private set; }
        
        private const float SPACE_BETWEEN_REGIMENT = 2.5f;
        private UnitFactory unitFactory;
        
        [SerializeField] private LayerMask PlayerUnitLayerMask;
        [field: SerializeField] public RegimentSpawner[] CreationOrders{ get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
            } 
            
            unitFactory = GetComponent<UnitFactory>();
            CreateRegiments(transform.position);
            Debug.Log("RegimentFactory Awake");
        }

        private void Start()
        {
            //CreateRegiments(transform.position);
        }

        public void CreateRegiments(Vector3 instancePosition)
        {
            List<Regiment> regiments = new (2);

            float offsetPosition = 0;

            for (int i = 0; i < CreationOrders.Length; i++)
            {
                RegimentSpawner currentSpawner = CreationOrders[i];
                RegimentSpawner previousSpawner = i == 0 ? currentSpawner : CreationOrders[i-1];

                float offset = GetOffset(currentSpawner, i) / 2f;
                offset += i == 0 ? 0 : GetOffset(previousSpawner, i) / 2f;
                offsetPosition += offset;

                for (int j = 0; j < CreationOrders[i].Number; j++)
                {
                    int regIndex = CreationOrders[i].Number * i + j;
                    offsetPosition += GetOffset(currentSpawner, j) + SPACE_BETWEEN_REGIMENT; //Careful it adds the const even if j=0!
                    Regiment regiment = InstantiateRegiment(regIndex, currentSpawner, instancePosition, offsetPosition);
                    regiments.Add(regiment);

                    regiment.UnitsTransform = unitFactory.CreateRegimentsUnit(regiment, CreationOrders[i].BaseNumUnit, CreationOrders[i].UnitPrefab).ToArray();
                    
                    //Directly on Units?
                    
                    Array.ForEach(regiment.UnitsTransform, unit => unit.gameObject.layer = math.floorlog2(PlayerUnitLayerMask));
                }
            }
            Array.Clear(CreationOrders, 0, CreationOrders.Length);
            //return regiments;
        }

        private Regiment InstantiateRegiment(int regimentIndex, RegimentSpawner spawner, Vector3 instancePosition, float offsetPosition)
        {
            //==================================================================================================
            //TEMP FOR AI
            //Vector3 center = GetComponent<RegimentManager>().transform.position;
            //==================================================================================================
            Vector3 position = instancePosition + offsetPosition * Vector3.right;
            
            GameObject newRegiment = new($"{regimentIndex}", typeof(Regiment));
            newRegiment.transform.SetPositionAndRotation(position, Quaternion.identity);

            Regiment regimentComponent = newRegiment.GetComponent<Regiment>();
            regimentComponent.OwnerID = spawner.OwnerID;
            regimentComponent.RegimentID = newRegiment.GetInstanceID();
            newRegiment.name = $"{regimentIndex}_{regimentComponent.RegimentID}";

            return regimentComponent;
        }

        private float GetOffset(RegimentSpawner spawner, int index)
        {
            const float spaceBtwUnits = 1.5f;
            int numRow = spawner.BaseNumUnit / 2;
            
            //DISTANCE from regiment to an other IS NOT divide by 2 !!
            if (index == 0) return 0;
            float offset = (numRow * spaceBtwUnits);
            return offset;
        }
    }
    
}