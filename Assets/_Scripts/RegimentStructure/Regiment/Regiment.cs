using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;

using static UnityEngine.Quaternion;


namespace KaizerWald
{
    public partial class Regiment : MonoBehaviour, ISelectable
    {
        public static readonly float FovAngleInDegrees = 30f;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private HashSet<Transform> DeadUnits;
        private TransformAccessWrapper<Unit> UnitsListWrapper;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [field:SerializeField] public bool IsPreselected { get; set; }
        [field:SerializeField] public bool IsSelected { get; set; }
        
        [field:SerializeField] public ulong OwnerID { get; private set; }
        [field: SerializeField] public int TeamID { get; private set; }
        [field:SerializeField] public int RegimentID { get; private set; }
        [field:SerializeField] public RegimentType RegimentType { get; private set; }
        
        public Formation CurrentFormation { get; private set; }
        public RegimentStateMachine StateMachine { get; private set; }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public List<Unit> Units => UnitsListWrapper.Datas;
        public List<Transform> UnitsTransform => UnitsListWrapper.Transforms;
        public TransformAccessArray UnitsTransformAccessArray => UnitsListWrapper.UnitsTransformAccessArray;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void OnDestroy()
        {
            UnitsListWrapper.Dispose();
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void Initialize(ulong ownerID, int teamID, UnitFactory unitFactory, RegimentSpawner currentSpawner, Vector3 direction, string regimentName = default)
        {
            InitializeProperties(ownerID, teamID, currentSpawner.RegimentType, direction, regimentName);
            CreateAndRegisterUnits(unitFactory);
            InitializeStateMachine();
        }

        private void InitializeProperties(ulong ownerID, int teamID, RegimentType regimentType, Vector3 direction, string regimentName = default)
        {
            name = regimentName ?? $"Player{ownerID}_Regiment{RegimentID}";
            OwnerID = ownerID;
            TeamID = teamID;
            RegimentID = transform.GetInstanceID();
            RegimentType = regimentType;
            CurrentFormation = new Formation(regimentType, direction);
        }

        private void CreateAndRegisterUnits(UnitFactory unitFactory)
        {
            List<Unit> units = unitFactory.CreateRegimentsUnit(this, RegimentType.RegimentClass.BaseNumberUnit, RegimentType.UnitPrefab);
            UnitsListWrapper = new TransformAccessWrapper<Unit>(units);
            DeadUnits = new HashSet<Transform>((int)(units.Count * 0.2f)); //almost impossible a regiment loose more than 20% of it's member during a frame
        }

        private void InitializeStateMachine()
        {
            StateMachine = this.GetOrAddComponent<RegimentStateMachine>();
            StateMachine.Initialize();
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Regiment Update Event ◈◈◈◈◈◈                                                                   ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnUpdate()
        {
            StateMachine.OnUpdate();
            Units.ForEach(unit => unit.UpdateUnit());
        }

        public void OnLateUpdate()
        {
            ClearDeadUnits();
        }

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Regiment Update Event ◇◇◇◇◇◇                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void OnDeadUnit(Unit unit)
        {
            DeadUnits.Add(unit.transform);
        }

        private void ClearDeadUnits()
        {
            if (DeadUnits.Count == 0) return;
            foreach (Transform deadUnit in DeadUnits)
            {
                UnitsListWrapper.Remove(deadUnit);
            }
            DeadUnits.Clear();
        }
    }
}
