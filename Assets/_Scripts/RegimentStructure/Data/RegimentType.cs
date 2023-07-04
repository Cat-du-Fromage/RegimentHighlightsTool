using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    [CreateAssetMenu(fileName = "NewRegimentType", menuName = "Regiment/RegimentType", order = 2)]
    public class RegimentType : ScriptableObject
    {
        public RegimentClass RegimentClass;
        public GameObject UnitPrefab;
    }
}
