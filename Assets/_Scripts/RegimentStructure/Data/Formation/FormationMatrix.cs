using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;

namespace Kaizerwald
{
    public class FormationMatrix<T> where T : MonoBehaviour
    {
        private List<T> elements;
        
        public TransformAccessArray FormationTransformAccessArray { get; private set; }
        
        // Fait la jonction pour les cas suivant
        // Suppression: Donc remove at SwapBack de TAA (Ne change pas
        public List<int> IndexToRealIndex { get; private set; }
        
        public Dictionary<T, int> ElementKeyIndex { get; private set; }

        public T this[int index] => elements[index];


        public FormationMatrix(List<T> units)
        {
            elements = units;
            Transform[] transforms = units.Select(unit => unit.transform).ToArray();
            FormationTransformAccessArray = new TransformAccessArray(transforms);
            
            IndexToRealIndex = new List<int>(units.Count);
            ElementKeyIndex = new Dictionary<T, int>(units.Count);
            for (int i = 0; i < units.Count; i++)
            {
                IndexToRealIndex.Add(i);
                ElementKeyIndex.Add(units[i], i);
            }
        }

        public void Swap(int lhs, int rhs)
        {
            elements.Swap(lhs, rhs);
            IndexToRealIndex.Swap(lhs, rhs);
            
        }
    }
}
