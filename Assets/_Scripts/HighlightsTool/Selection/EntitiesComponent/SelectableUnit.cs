using UnityEngine;

namespace KaizerWald
{
    public class SelectableUnit : MonoBehaviour
    {
        public SelectableRegiment SelectableRegimentAttach { get; private set; }
        public void SetRegiment(SelectableRegiment regiment) => SelectableRegimentAttach = regiment;
    }
}