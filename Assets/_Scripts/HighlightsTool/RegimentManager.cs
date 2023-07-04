using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;

namespace KaizerWald
{
    public class RegimentManager : HighlightCoordinator
    {
        private RegimentFactory factory;
        public List<Regiment> Regiments { get; private set; }
        public event Action<Regiment> OnNewRegiment; 
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Unity Events ◇◇◇◇◇                                                                                   ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected override void Awake()
        {
            base.Awake();
            Regiments = new List<Regiment>();
            factory = FindObjectOfType<RegimentFactory>();
        }

        private void OnEnable()
        {
            factory.OnRegimentCreated += RegisterNewRegiment;
        }

        private void OnDisable()
        {
            factory.OnRegimentCreated -= RegisterNewRegiment;
            foreach (Delegate action in OnNewRegiment?.GetInvocationList()!)
            {
                OnNewRegiment -= (Action<Regiment>)action;
            }
        }
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Class Methods ◇◇◇◇◇                                                                                  ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void RegisterNewRegiment(Regiment regiment)
        {
            Regiments.Add(regiment);
            OnNewRegiment?.Invoke(regiment);
        }
    }
}
