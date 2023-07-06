using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class RegimentState : IRegimentState
    {
        public abstract void HandleInput();
        public abstract IRegimentState UpdateState(Regiment regiment);
        public abstract void OnEnter(Regiment regiment);
        public abstract void OnUpdate();
        public abstract void OnExit(Regiment regiment);
    }
}
