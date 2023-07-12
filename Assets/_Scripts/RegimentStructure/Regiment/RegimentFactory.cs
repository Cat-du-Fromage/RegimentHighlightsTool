using System;
using System.Collections;
using System.Collections.Generic;
//using KWUtils;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace KaizerWald
{
    [Serializable]
    public struct RegimentSpawner
    {
        public ulong OwnerID;
        public int Number;
        public RegimentType RegimentType;
        //public GameObject UnitPrefab;
    }

    [RequireComponent(typeof(UnitFactory))]
    public sealed class RegimentFactory : MonoBehaviourSingleton<RegimentFactory>
    {
        private const float SPACE_BETWEEN_REGIMENT = 2.5f;
        private UnitFactory unitFactory;
        [field: SerializeField] public RegimentSpawner[] CreationOrders { get; private set; }

        public event Action<Regiment> OnRegimentCreated; 

        protected override void Awake()
        {
            base.Awake();
            unitFactory = GetComponent<UnitFactory>();
        }

        private void Start()
        {
            CreateRegiments(transform.position);
        }

        public void CreateRegiments(Vector3 defaultInstancePosition)
        {
            float offsetPosition = 0;

            for (int i = 0; i < CreationOrders.Length; i++)
            {
                (Transform spawnerTransform, Vector3 instancePosition) = GetInstancePosition(CreationOrders[i].OwnerID);
                
                RegimentSpawner currentSpawner = CreationOrders[i];
                RegimentSpawner previousSpawner = i is 0 ? currentSpawner : CreationOrders[i-1];

                float offset = GetOffset(currentSpawner, i) + GetOffset(previousSpawner, i);
                offsetPosition += offset;
                
                for (int j = 0; j < currentSpawner.Number; j++) //same regiment creation
                {
                    offsetPosition += OffsetSameRegiment(j, currentSpawner.RegimentType.RegimentClass) + SPACE_BETWEEN_REGIMENT; //Careful it adds the const even if j=0!
                    Regiment regiment = InstantiateRegiment(spawnerTransform, instancePosition, offsetPosition);
                    regiment.Initialize(currentSpawner.OwnerID, unitFactory, currentSpawner, regiment.transform.forward);
                    OnRegimentCreated?.Invoke(regiment);
                }
            }
            Array.Clear(CreationOrders, 0, CreationOrders.Length);
        }

        private (Transform, Vector3) GetInstancePosition(ulong ownerID)
        {
            int playerIndex = (int)ownerID;
            if (playerIndex is < 0 or > 1) return (transform, transform.position);
            Vector3 instancePosition = TerrainManager.Instance.GetPlayerFirstSpawnPosition(playerIndex);
            return (TerrainManager.Instance.GetSpawnerTransform(playerIndex), instancePosition);
        }

        private Regiment InstantiateRegiment(Transform spawnerTransform, Vector3 instancePosition, float offsetPosition)
        {
            Vector3 position = instancePosition + offsetPosition * spawnerTransform.right;
            GameObject newRegiment = new($"DefaultRegiment", typeof(Regiment));
            Regiment regiment = newRegiment.GetComponent<Regiment>();
            regiment.transform.SetPositionAndRotation(position, spawnerTransform.localRotation);
            return regiment;
        }

        private float GetOffset(RegimentSpawner spawner, int index)
        {
            if (index == 0) return 0;
            RegimentClass regimentClass = spawner.RegimentType.RegimentClass;
            float spaceBtwUnits = regimentClass.SpaceBetweenUnits + regimentClass.Category.UnitSize.x;
            return regimentClass.MaxRow * spaceBtwUnits  * 0.5f;
        }

        private float OffsetSameRegiment(int index, RegimentClass regimentClass)
        {
            if (index == 0) return 0;
            float spaceBtwUnits = regimentClass.SpaceBetweenUnits + regimentClass.Category.UnitSize.x;
            return regimentClass.MaxRow * spaceBtwUnits; //DISTANCE from same regiment to an other IS NOT divide by 2 !!
        }
    }
    
}