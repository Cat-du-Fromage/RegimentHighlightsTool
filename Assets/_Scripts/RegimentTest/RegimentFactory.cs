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
        public int BaseNumUnit;
        public GameObject UnitPrefab;
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

        public void CreateRegiments(Vector3 instancePosition)
        {
            float offsetPosition = 0;
            for (int i = 0; i < CreationOrders.Length; i++)
            {
                RegimentSpawner currentSpawner = CreationOrders[i];
                RegimentSpawner previousSpawner = i == 0 ? currentSpawner : CreationOrders[i-1];

                float offset = GetOffset(currentSpawner, i) * 0.5f;
                offset += GetOffset(previousSpawner, i) * 0.5f;
                offsetPosition += offset;

                for (int j = 0; j < currentSpawner.Number; j++) //same regiment creation
                {
                    offsetPosition += GetOffset(currentSpawner, j) + SPACE_BETWEEN_REGIMENT; //Careful it adds the const even if j=0!
                    Regiment regiment = InstantiateRegiment(instancePosition, offsetPosition);
                    regiment.Initialize(currentSpawner.OwnerID, unitFactory, currentSpawner);
                    OnRegimentCreated?.Invoke(regiment);
                }
            }
            Array.Clear(CreationOrders, 0, CreationOrders.Length);
        }

        private Regiment InstantiateRegiment(Vector3 instancePosition, float offsetPosition)
        {
            Vector3 position = instancePosition + offsetPosition * Vector3.right;
            GameObject newRegiment = new($"DefaultRegiment", typeof(Regiment));
            newRegiment.transform.SetPositionAndRotation(position, Quaternion.identity);
            return newRegiment.GetComponent<Regiment>();
        }

        private float GetOffset(RegimentSpawner spawner, int index)
        {
            const float spaceBtwUnits = 1.5f;
            int numRow = (int)(spawner.BaseNumUnit * 0.5f);
            return index == 0 ? 0 : numRow * spaceBtwUnits; //DISTANCE from regiment to an other IS NOT divide by 2 !!
        }
    }
    
}