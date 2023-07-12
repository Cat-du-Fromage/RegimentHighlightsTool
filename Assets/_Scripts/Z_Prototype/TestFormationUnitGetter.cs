using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace KaizerWald
{
    public class TestFormationUnitGetter : MonoBehaviour
    {
        [SerializeField] private int NumUnits = 20;
        [SerializeField] private int FormationWidth = 4;
        private const float RADIUS = 0.5f;
        private Formation formation;
        public bool DebugTest = false;

        private bool formationAssigned = false;
        private Transform regTransform;

        private void Awake()
        {
            regTransform = transform;
            DebugTest = false;
        }

        private void Start()
        {
            InitFormation();
            DebugTest = true;
        }

        
        private void Update()
        {
            if (!formationAssigned) return;
            if(formation.Width != FormationWidth) formation.SetWidth(Mathf.Max(1, FormationWidth));
            
            CheckFormation();
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (!GetIntersectionPointOnTerrain(out RaycastHit hit)) return;
                float3 newDir = math.normalizesafe(hit.point - regTransform.position);
                formation.SetDirection(newDir.xz);
                regTransform.position = hit.point;
            }
        }
/*
        private void LateUpdate()
        {
            if (formationAssigned) return;
            formation = new FormationData(FindFirstObjectByType<Regiment>().CurrentFormation);
            FormationWidth = formation.Width;
            formationAssigned = true;
            Debug.Log(formation.ToString());
        }
*/
        private void InitFormation()
        {
            formation = new Formation(NumUnits);
            formation.SetWidth(FormationWidth);
            formation.SetDirection(((float3)regTransform.forward).xz);
            formationAssigned = true;
        }

        private void CheckFormation()
        {
            if(FormationWidth > NumUnits) FormationWidth = NumUnits;
            if(formation.Width != FormationWidth) formation.SetWidth(Mathf.Max(1, FormationWidth));
            if (formation.NumUnitsAlive != NumUnits)
            {
                int unitsAlive = formation.NumUnitsAlive;
                formation.Add(NumUnits > unitsAlive ? NumUnits - unitsAlive : unitsAlive - NumUnits);
            }
        }
        
        private bool GetIntersectionPointOnTerrain(out RaycastHit hit)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
            return Physics.Raycast(ray, out hit, Mathf.Infinity, 1<<8);
        }
        
        private void OnDrawGizmos()
        {
            if (!DebugTest || !formationAssigned) return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, RADIUS);
            float2[] positions = ((FormationData)formation).GetUnitsPositionRelativeToRegiment((float3)regTransform.position);

            Gizmos.color = Color.green;
            foreach (float2 unitPos2D in positions)
            {
                Vector3 unitPos = new Vector3(unitPos2D.x,regTransform.position.y, unitPos2D.y);
                Gizmos.DrawSphere(unitPos, RADIUS);
            }
        }
    }
}
