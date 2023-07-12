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

        public int Range = 0;
        public int Accuracy = 0;
        public int ReloadingSkill = 0;
        public int Speed = 1;
        public int Moral = 1;
    }
}
