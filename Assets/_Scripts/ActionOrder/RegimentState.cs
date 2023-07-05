using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public interface IRegimentState
    {
        public IRegimentState UpdateState(Regiment regiment);
        public void OnEnter(Regiment regiment);
        public void OnExit(Regiment regiment);
    }
    public abstract class RegimentState : IRegimentState
    {
        public abstract IRegimentState UpdateState(Regiment regiment);

        public abstract void OnEnter(Regiment regiment);

        public abstract void OnExit(Regiment regiment);
    }
}
