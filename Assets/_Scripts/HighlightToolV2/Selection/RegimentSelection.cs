using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald2
{
    public sealed class RegimentSelection : CompositeSystem
    {
        private HighlightSubSystem PreselectionSubSystem;
        private HighlightSubSystem SelectionSubSystem;

        public override CombinedController Controller { get; protected set; }
        public override HighlightSubSystem SubSystem1 => PreselectionSubSystem;
        public override HighlightSubSystem SubSystem2 => SelectionSubSystem;
        
        public RegimentSelection(HighlightMediator coord, PlayerControls controls, LayerMask selectionLayer, GameObject preselectPrefab, GameObject selectPrefab)
        {
            PreselectionSubSystem = new PreselectionSubSystem(this, preselectPrefab);
            SelectionSubSystem = new SelectionSubSystem(this, selectPrefab);
            Controller = new RegimentSelectionController(coord, this, Camera.main, controls, selectionLayer);
        }
    }
}
