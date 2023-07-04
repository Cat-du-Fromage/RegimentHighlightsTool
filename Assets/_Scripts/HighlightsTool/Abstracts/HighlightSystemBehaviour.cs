using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class HighlightSystemBehaviour<T> : MonoBehaviour
    where T : HighlightCoordinator
    {
        public abstract T RegimentManager { get; protected set; }
        public HighlightCoordinator Coordinator { get; protected set; }
        public HashSet<Regiment> AllRegiments { get; protected set; }
        
        protected virtual void Awake()
        {
            AllRegiments = new HashSet<Regiment>(2);
            RegimentManager = FindAnyObjectByType<T>();
            Coordinator = (HighlightCoordinator)RegimentManager;
        }

        public virtual void RegisterRegiment(Regiment regiment)
        {
            AllRegiments.Add(regiment);
        }

        public virtual void UnregisterRegiment(Regiment regiment)
        {
            AllRegiments.Remove(regiment);
        }
    }
}