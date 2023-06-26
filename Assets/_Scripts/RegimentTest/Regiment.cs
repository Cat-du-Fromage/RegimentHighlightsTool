using UnityEngine;

namespace KaizerWald
{
    
    public class Regiment : MonoBehaviour
    {
        //Interface
        public bool IsPreselected { get; set; }
        public bool IsSelected { get; set; }
        
        //Properties
        public ulong OwnerID { get; set; }
        public int RegimentID { get; set; }
        public Transform[] UnitsTransform { get; set; }
        
        public SelectableRegiment SelectableRegimentComponent;

        public Regiment Initialize(ulong ownerID, UnitFactory unitFactory, RegimentSpawner currentSpawner, string regimentName = default)
        {
            OwnerID = ownerID;
            RegimentID = transform.GetInstanceID();
            UnitsTransform = unitFactory.CreateRegimentsUnit(this, currentSpawner.BaseNumUnit, currentSpawner.UnitPrefab).ToArray();
            name = regimentName ?? $"Player{ownerID}_Regiment{RegimentID}";
            return this;
        }
    }
    
}
