using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    /// <summary>
    /// REFACTOR: Make it monobehaviour component
    /// </summary>
    public class SelectableRegiment : MonoBehaviour
    {
        [field:SerializeField] public bool IsPreselected { get; private set; }
        [field:SerializeField] public bool IsSelected { get; private set; }
        
        public ulong OwnerID { get; private set; }
        public int RegimentID { get; private set; }
        public Transform[] UnitsTransform { get; private set; }

        private void Awake()
        {
            Regiment regimentComponent = GetComponent<Regiment>();
            OwnerID = regimentComponent.OwnerID;
            RegimentID = regimentComponent.RegimentID;
        }

        public void RegisterUnits<TUnit>(List<TUnit> units)
        where TUnit : MonoBehaviour
        {
            UnitsTransform = new Transform[units.Count];
            for (int i = 0; i < units.Count; i++)
            {
                UnitsTransform[i] = units[i].transform;
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