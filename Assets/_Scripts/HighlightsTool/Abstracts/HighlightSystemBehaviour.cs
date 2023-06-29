using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class HighlightSystemBehaviour<T> : MonoBehaviour
    where T : HighlightCoordinator
    {
        public abstract T RegimentManager { get; protected set; }
        public HighlightCoordinator Coordinator { get; protected set; }
        public HashSet<SelectableRegiment> SelectablesRegiments { get; protected set; }
        
        protected virtual void Awake()
        {
            SelectablesRegiments = new HashSet<SelectableRegiment>(2);
            RegimentManager = FindAnyObjectByType<T>();
            Coordinator = (HighlightCoordinator)RegimentManager;
        }

        public virtual void RegisterRegiment(SelectableRegiment regiment)
        {
            SelectablesRegiments.Add(regiment);
        }

        public virtual void UnregisterRegiment(SelectableRegiment regiment)
        {
            SelectablesRegiments.Remove(regiment);
        }
        
        protected void PopulateHighlights<TRegiment, TUnit>(TRegiment regiment, List<TUnit> units)
        where TUnit : MonoBehaviour
        where TRegiment : MonoBehaviour
        {
            SelectableRegiment selectableRegiment = regiment.GetOrAddComponent<SelectableRegiment>();
            
            if (units == null) return;
            selectableRegiment.RegisterUnits(units);
            units.ForEach(unit => unit.GetOrAddComponent<SelectableUnit>().SetRegiment(selectableRegiment));
            RegisterRegiment(selectableRegiment);
        }
    }
}