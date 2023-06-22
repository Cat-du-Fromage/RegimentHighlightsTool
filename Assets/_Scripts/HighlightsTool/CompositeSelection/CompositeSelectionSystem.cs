using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public class CompositeSelectionSystem : HighlightSystem
    {
        public CompositeSelectionSystem(HighlightCoordinator coordinator, GameObject defaultPrefab) : base(coordinator, defaultPrefab)
        {
            
        }
    }
}
