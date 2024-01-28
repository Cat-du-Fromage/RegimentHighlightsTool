using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Kaizerwald
{
    public class HighlightRegiment : MonoBehaviour, ISelectable
    {
        [field:SerializeField] public ulong OwnerID { get; private set; }
        [field: SerializeField] public int TeamID { get; private set; }
        [field:SerializeField] public int RegimentID { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private Transform regimentTransform;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public float3 DestinationLeaderPosition { get; private set; }
        
        public Formation CurrentFormation { get; private set; }
        
        public Formation DestinationFormation { get; private set; }

        public List<HighlightUnit> HighlightUnits { get; private set; }
        
        public List<Transform> HighlightUnitsTransform { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public float3 RegimentPosition => regimentTransform.position;
        public float3 CurrentLeaderPosition => regimentTransform.position;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ ISelectable ◈◈◈◈◈◈                                                                                      ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        [field:SerializeField] public bool IsPreselected { get; set; }
        [field:SerializeField] public bool IsSelected { get; set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            regimentTransform = transform;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void InitializeProperties<T>(ulong ownerID, int teamID, List<T> units, int2 minMaxRow, float2 unitSize, float spaceBetweenUnit, float3 direction)
        where T : MonoBehaviour
        {
            RegimentID = transform.GetInstanceID();
            OwnerID = ownerID;
            TeamID = teamID;
            CurrentFormation = new Formation(units.Count, minMaxRow, unitSize, spaceBetweenUnit, direction);
            DestinationFormation = CurrentFormation;
            CreateAndRegisterHighlightUnits(units);
        }
        
        private void CreateAndRegisterHighlightUnits<T>(List<T> units)
        where T : MonoBehaviour
        {
            HighlightUnits = new List<HighlightUnit>(units.Count);
            HighlightUnitsTransform = new List<Transform>(units.Count);
            foreach (T unit in units)
            {
                AddUnit(unit);
            }
        }

        public void AddUnit<T>(T unit)
        where T : MonoBehaviour
        {
            HighlightUnit highlightUnit = unit.gameObject.AddComponent<HighlightUnit>();
            highlightUnit.AttachToRegiment(this);
            HighlightUnits.Add(highlightUnit);
            HighlightUnitsTransform.Add(highlightUnit.transform);
        }

        public void RemoveUnit(HighlightUnit highlightUnit)
        {
            int index = HighlightUnits.IndexOf(highlightUnit);
            if (index == -1) return;
            HighlightUnits.RemoveAt(index);
            HighlightUnitsTransform.RemoveAt(index);
        }

        public void SetDestination(float3 destinationLeaderPosition, FormationData formationDestination)
        {
            DestinationLeaderPosition = destinationLeaderPosition;
            DestinationFormation = formationDestination;
        }
    }
}
