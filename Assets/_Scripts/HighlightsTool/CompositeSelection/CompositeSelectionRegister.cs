using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class HighlightContainer
    {
        private int EntityPerLayer;
        private int LayerDepth;
        public Dictionary<int, List<HighlightBehaviour>> Records { get; private set; }

        public HighlightContainer()
        {
            
        }
    }
    
    public class CompositeSelectionRegister : HighlightRegister
    {
        public CompositeSelectionRegister(HighlightSystem system, GameObject highlightPrefab) : base(system, highlightPrefab)
        {
        }
    }
}
