using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class Unit : MonoBehaviour
    {
        [field:SerializeField] public bool IsPreselected { get; private set; }
        [field:SerializeField] public bool IsSelected { get; private set; }
        
        public Regiment RegimentAttach { get; private set; }
        public int IndexInRegiment { get; set; } = 1;
        
        public void SetRegiment(Regiment regiment)
        {
            RegimentAttach = regiment;
        }
    }
}