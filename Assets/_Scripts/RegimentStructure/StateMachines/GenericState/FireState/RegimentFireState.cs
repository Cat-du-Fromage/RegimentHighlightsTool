using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class RegimentFireState : FireState<Regiment>
    {
        public RegimentFireState(Regiment objectAttach) : base(objectAttach)
        {
            Target = null;
        }
        
        
    }
}
