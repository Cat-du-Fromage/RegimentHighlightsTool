using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public sealed class HighlightsManager : HighlightCoordinator
    {
        protected override void Awake()
        {
            base.Awake();
            Debug.Log("HighlightsManager Awake");
        }
        
        protected override void Start()
        {
            base.Start();
            Debug.Log("HighlightsManager Start");
        }
    }
}
