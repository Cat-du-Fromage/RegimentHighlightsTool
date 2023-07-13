using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.Mathf;

namespace KaizerWald
{
    public class RegimentFieldOfView : MonoBehaviour
    {
        [field:SerializeField] public float ViewRadius { get; private set; }
        [field:SerializeField, Range(0,360)] public float ViewAngle { get; private set; }
        
        public Vector3 GetDirectionFromAngle(float angleInDegree, bool isGlobalAngle)
        {
            angleInDegree += isGlobalAngle ? transform.eulerAngles.y : 0;
            float angleInRadian = angleInDegree * Deg2Rad;
            return new Vector3(Sin(angleInRadian), 0, Cos(angleInRadian));
        }
    }
}
