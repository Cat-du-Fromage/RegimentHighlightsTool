using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Jobs;

using static UnityEngine.Quaternion;
using static Unity.Mathematics.math;
using static KaizerWald.FormationUtils;

namespace KaizerWald
{
    public class RegimentHighlightManager : MonoBehaviourSingleton<RegimentHighlightManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }
    }
}
