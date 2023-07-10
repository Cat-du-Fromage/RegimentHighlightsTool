using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;

namespace KaizerWald
{
    public class Regiment : MonoBehaviour, ISelectable
    {
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
        [field:SerializeField] public int RegimentID { get; private set; }
        [field:SerializeField] public RegimentType RegimentType { get; private set; }
        
        [field:SerializeField] public FormationData CurrentFormation { get; private set; }

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
        public void Initialize(ulong ownerID, UnitFactory unitFactory, RegimentSpawner currentSpawner, Vector3 direction, string regimentName = default)
        {
            name = regimentName ?? $"Player{ownerID}_Regiment{RegimentID}";
            OwnerID = ownerID;
            RegimentID = transform.GetInstanceID();
            RegimentType = currentSpawner.RegimentType;
            CurrentFormation = new FormationData(currentSpawner.RegimentType, direction);
            CreateAndRegisterUnits(unitFactory);

            InitializeStates();
        }

        private void CreateAndRegisterUnits(UnitFactory unitFactory)
        {
            List<Unit> units = unitFactory.CreateRegimentsUnit(this, RegimentType.RegimentClass.BaseNumberUnit, RegimentType.UnitPrefab);
            UnitsListWrapper = new TransformAccessWrapper<Unit>(units);
            DeadUnits = new HashSet<Transform>((int)(units.Count * 0.2f)); //almost impossible a regiment loose more than 20% of it's member during a frame
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Regiment Update Event ◈◈◈◈◈◈                                                                   ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnUpdate()
        {
            CurrentState?.OnStateUpdate();
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
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE MACHINE ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public EStates currentState;
        public Dictionary<EStates, RegimentState> States;

        private RegimentState CurrentState => States[currentState];

        private void InitializeStates()
        {
            States = new Dictionary<EStates, RegimentState>()
            {
                {EStates.Idle, new RegimentIdleState(this)},
                {EStates.Move, new RegimentMoveState(this)}
            };
            currentState = EStates.Idle;
        }

        public void TransitionState(EStates newState)
        {
            CurrentState.OnStateExit();
            currentState = newState;
            CurrentState.OnStateEnter();
        }
        
        public void OnMoveOrderReceived(RegimentOrder order)
        {
            currentState = order.StateOrdered;
            switch (currentState)
            {
                case EStates.Idle:
                    return;
                case EStates.Move:
                    CurrentState.OnOrderEnter(order);
                    foreach (Unit unit in Units)
                    {
                        unit.OnOrderReceived(order);
                    }
                    return;
                default:
                    return;
            }
            
            //currentState = new RegimentMoveState(this);
            //currentState.OnOrderEnter(order);
        }
    }
}
