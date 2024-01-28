using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    [CreateAssetMenu(fileName = "NewRegimentClass", menuName = "Regiment/Class", order = 1)]
    public class RegimentClass : ScriptableObject
    {
        public RegimentCategory Category;

        public int BaseNumberUnit = 20;
        public int MinRow = 4;
        public int MaxRow = 10;
        public float SpaceBetweenUnits = 0.5f;
    }
}
