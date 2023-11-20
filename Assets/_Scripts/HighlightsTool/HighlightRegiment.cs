using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class HighlightRegiment : MonoBehaviour
    {
        [field:SerializeField] public ulong OwnerID { get; private set; }
        [field: SerializeField] public int TeamID { get; private set; }
        [field:SerializeField] public int RegimentID { get; private set; }
        public Formation CurrentFormation { get; private set; }

        public List<HighlightUnit> HighlightUnits;
        
        private void CreateAndRegisterUnits(int numUnits)
        {
            List<HighlightUnit> units = new List<HighlightUnit>(numUnits);
            
        }

        private void InitializeProperties(ulong ownerID, int teamID, RegimentType regimentType, Vector3 direction, string regimentName = default)
        {
            RegimentID = transform.GetInstanceID();
            name = regimentName ?? $"Player{ownerID}_Regiment{RegimentID}";
            OwnerID = ownerID;
            TeamID = teamID;
            CurrentFormation = new Formation(regimentType, direction);
        }
    }
}
