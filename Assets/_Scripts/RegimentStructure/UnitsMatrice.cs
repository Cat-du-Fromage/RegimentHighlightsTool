using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class UnitsMatrix
    {
        public Unit[] Units { get; private set; }
        public Transform[] UnitsTransform { get; private set; }
        
        public UnitsMatrix(List<Unit> units)
        {
            Units = new Unit[units.Count];
            UnitsTransform = new Transform[units.Count];
            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                Units[i] = unit;
                UnitsTransform[i] = unit.transform;
            }
        }

        public void RemoveAt(int index)
        {
            
        }
    }
}
