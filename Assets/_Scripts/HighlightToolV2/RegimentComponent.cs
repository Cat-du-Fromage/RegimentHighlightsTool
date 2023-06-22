using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald2
{
    public class RegimentComponent : MonoBehaviour
    {
        public bool IsPreselected { get; set; }
        public bool IsSelected { get; set; }
        
        public int RegimentID { get; }
        public ulong OwnerID { get; }
        
        public Transform[] UnitsTransform { get; set; }
    }
}
