using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWald
{
    [CreateAssetMenu(fileName = "NewRegimentType", menuName = "Regiment/RegimentType", order = 2)]
    public class RegimentType : ScriptableObject, IFormationInfo
    {
        public RegimentClass RegimentClass;
        public GameObject UnitPrefab;
        public GameObject BulletPrefab;
        
        public int Range = 0;
        public int Accuracy = 0;
        public int ReloadingSkill = 0;
        public int Speed = 1;
        public int Moral = 1;

        //Interface
        public int BaseNumUnits => RegimentClass.BaseNumberUnit;
        public int2 MinMaxRow => new (RegimentClass.MinRow, RegimentClass.MaxRow);
        public float2 UnitSize => new (RegimentClass.Category.UnitSize.x, RegimentClass.Category.UnitSize.z);
        public float SpaceBetweenUnit => RegimentClass.SpaceBetweenUnits;
    }
}
