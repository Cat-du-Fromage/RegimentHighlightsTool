using UnityEngine;

namespace KaizerWald
{
    public class Regiment : MonoBehaviour, ISelectableRegiment
    {
        //Interface
        public bool IsPreselected { get; set; }
        public bool IsSelected { get; set; }
        
        //Properties
        public ulong OwnerID { get; set; }
        public int RegimentID { get; set; }
        public Transform[] UnitsTransform { get; set; }
    }
}
