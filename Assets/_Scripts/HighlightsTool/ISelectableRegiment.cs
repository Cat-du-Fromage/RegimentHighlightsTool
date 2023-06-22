using UnityEngine;

namespace KaizerWald
{
    public interface ISelectableRegiment
    {
        public ulong OwnerID { get; }
        public int RegimentID { get; }
        public bool IsPreselected { get; set; }
        public bool IsSelected { get; set; }
        public Transform[] UnitsTransform { get; set; }
    }
}