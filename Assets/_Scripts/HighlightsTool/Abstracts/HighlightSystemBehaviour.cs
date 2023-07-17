using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class HighlightSystemBehaviour<T> : MonoBehaviour
    where T : HighlightCoordinator
    {
        public abstract T RegimentManager { get; protected set; }
        public HighlightCoordinator Coordinator { get; protected set; }

        protected virtual void Awake()
        {
            //AllRegiments = new HashSet<Regiment>(2);
            RegimentManager = FindAnyObjectByType<T>();
            Coordinator = (HighlightCoordinator)RegimentManager;
        }

        public abstract void RegisterRegiment(Regiment regiment);

        public abstract void UnregisterRegiment(Regiment regiment);

        public abstract void ResizeBuffers(int regimentID, int numDead);
    }
}