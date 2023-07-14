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
        public int TeamID;
        public ulong OwnerID;
        public int Number;
        public RegimentType RegimentType;
    }

    [RequireComponent(typeof(UnitFactory))]
    public sealed class RegimentFactory : MonoBehaviourSingleton<RegimentFactory>
    {
        private const float SPACE_BETWEEN_REGIMENT = 2.5f;
        
        private UnitFactory unitFactory;
        private Transform factoryTransform;
        [field: SerializeField] public RegimentSpawner[] CreationOrders { get; private set; }

        public event Action<Regiment> OnRegimentCreated; 

        protected override void Awake()
        {
            factoryTransform = transform;
            base.Awake();
            unitFactory = GetComponent<UnitFactory>();
        }

        private void Start()
        {
            CreateRegiments();
        }


        private void CreateRegiments()
        {
            Dictionary<int, List<RegimentSpawner>> spawnerByTeam = DivideSpawnOrdersByTeam();
            foreach ((int teamId, List<RegimentSpawner> spawners) in spawnerByTeam)
            {
                float offsetPosition = 0;

                for (int i = 0; i < spawners.Count; i++)
                {
                    (Transform spawnerTransform, Vector3 instancePosition) = GetInstancePosition(teamId);
                
                    RegimentSpawner currentSpawner = spawners[i];
                    RegimentSpawner previousSpawner = i is 0 ? currentSpawner : spawners[i-1];

                    float offset = GetOffset(currentSpawner, i) + GetOffset(previousSpawner, i);
                    offsetPosition += offset;
                
                    for (int j = 0; j < currentSpawner.Number; j++) //same regiment creation
                    {
                        offsetPosition += OffsetSameRegiment(j, currentSpawner.RegimentType.RegimentClass) + SPACE_BETWEEN_REGIMENT; //Careful it adds the const even if j=0!
                        Regiment regiment = InstantiateRegiment(spawnerTransform, instancePosition, offsetPosition);
                        regiment.Initialize(currentSpawner.OwnerID, currentSpawner.TeamID, unitFactory, currentSpawner, regiment.transform.forward);
                        OnRegimentCreated?.Invoke(regiment);
                    }
                }
            }
        }

        private Dictionary<int, List<RegimentSpawner>> DivideSpawnOrdersByTeam()
        {
            Dictionary<int, List<RegimentSpawner>> spawnerByTeam = new (4);
            for (int i = 0; i < CreationOrders.Length; i++)
            {
                spawnerByTeam.AddSafe(CreationOrders[i].TeamID, CreationOrders[i]);
            }
            return spawnerByTeam;
        }

        private (Transform, Vector3) GetInstancePosition(int teamId)
        {
            if (teamId is < 0 or > 1) return (factoryTransform, factoryTransform.position);
            Vector3 instancePosition = TerrainManager.Instance.GetPlayerFirstSpawnPosition(teamId);
            return (TerrainManager.Instance.GetSpawnerTransform(teamId), instancePosition);
        }
        
        private (Transform, Vector3) GetInstancePosition(ulong ownerId)
        {
            int playerIndex = (int)ownerId;
            if (playerIndex is < 0 or > 1) return (factoryTransform, factoryTransform.position);
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

/*
//ISSUE! we introduce team BUT offset still apply to each regiment as if they were all from the same team
//Need to separate creations by team OR find a way to store different offset(1 by team)
public void CreateRegiments()
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
            regiment.Initialize(currentSpawner.OwnerID, currentSpawner.TeamID, unitFactory, currentSpawner, regiment.transform.forward);
            OnRegimentCreated?.Invoke(regiment);
        }
    }
    Array.Clear(CreationOrders, 0, CreationOrders.Length);
}
*/