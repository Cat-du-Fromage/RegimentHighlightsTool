using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class Unit : MonoBehaviour
    {
        public Regiment RegimentAttach { get; private set; }
        public int IndexInRegiment { get; set; } = 1;
        
        public void SetRegiment(Regiment regiment)
        {
            RegimentAttach = regiment;
        }
    }
}