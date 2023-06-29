using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    
    public class Regiment : MonoBehaviour
    {
        public ulong OwnerID { get; private set; }
        public int RegimentID { get; private set; }
        public List<Unit> Units { get; private set; }

        public void Initialize(ulong ownerID, UnitFactory unitFactory, RegimentSpawner currentSpawner, string regimentName = default)
        {
            OwnerID = ownerID;
            RegimentID = transform.GetInstanceID();
            name = regimentName ?? $"Player{ownerID}_Regiment{RegimentID}";
            Units = unitFactory.CreateRegimentsUnit(this, currentSpawner.BaseNumUnit, currentSpawner.UnitPrefab);
        }
    }
    
}
