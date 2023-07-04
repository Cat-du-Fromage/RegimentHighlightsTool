using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    
    public class Regiment : MonoBehaviour
    {
        [field:SerializeField] public bool IsPreselected { get; private set; }
        [field:SerializeField] public bool IsSelected { get; private set; }
        
        [field:SerializeField] public RegimentType RegimentType { get; private set; }
        public ulong OwnerID { get; private set; }
        public int RegimentID { get; private set; }
        public FormationData Formation { get; private set; }
        public List<Unit> Units { get; private set; }

        public Transform[] UnitsTransform { get; private set; }
        
        public void Initialize(ulong ownerID, UnitFactory unitFactory, RegimentSpawner currentSpawner, string regimentName = default)
        {
            OwnerID = ownerID;
            RegimentID = transform.GetInstanceID();
            name = regimentName ?? $"Player{ownerID}_Regiment{RegimentID}";
            
            RegimentType = currentSpawner.RegimentType;
            Formation = new FormationData(currentSpawner.RegimentType);
            
            CreateAndRegisterUnits(unitFactory);
        }

        private void CreateAndRegisterUnits(UnitFactory unitFactory)
        {
            Units = unitFactory.CreateRegimentsUnit(this, RegimentType.RegimentClass.BaseNumberUnit, RegimentType.UnitPrefab);
            UnitsTransform = new Transform[Units.Count];
            for (int i = 0; i < Units.Count; i++)
            {
                Units[i].SetRegiment(this);
                UnitsTransform[i] = Units[i].transform;
            }
        }
        
        public void SetSelectableProperties(ESelection index, bool value)
        {
            switch (index)
            {
                case ESelection.Preselection:
                    IsPreselected = value;
                    return;
                case ESelection.Selection:
                    IsSelected = value;
                    return;
                default:
                    return;
            }
        }
    }
    
}
