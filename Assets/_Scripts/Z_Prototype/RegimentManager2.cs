using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kaizerwald
{
    public class RegimentManager2 : MonoBehaviour
    {
        private HashSet<Regiment> regiments;
        private void Awake()
        {
            Regiment[] regiments2 = FindObjectsByType<Regiment>(FindObjectsSortMode.None);
            regiments = new HashSet<Regiment>(regiments2.Length);
            for (int i = 0; i < regiments2.Length; i++)
            {
                regiments.Add(regiments2[i]);
            }
        }

        // Update is called once per frame
        private void Update()
        {
        
        }

        private void MoveRegiment(Regiment regiment)
        {
            
        }
    }
}
