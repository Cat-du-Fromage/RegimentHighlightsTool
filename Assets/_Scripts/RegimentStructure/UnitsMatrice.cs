using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class UnitsMatrix
    {
        public Unit[] Units { get; private set; }
        public Transform[] UnitsTransform { get; private set; }
        
        public UnitsMatrix(List<Unit> datas)
        {
            Units = new Unit[datas.Count];
            UnitsTransform = new Transform[datas.Count];
            for (int i = 0; i < datas.Count; i++)
            {
                Unit unit = datas[i];
                Units[i] = unit;
                UnitsTransform[i] = unit.transform;
            }
        }

        public void RemoveAt(int index)
        {
            
        }
    }
}
