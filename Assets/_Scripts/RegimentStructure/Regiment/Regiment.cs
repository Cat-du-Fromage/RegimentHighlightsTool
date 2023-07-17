using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;

using static UnityEngine.Quaternion;


namespace KaizerWald
{
    public partial class Regiment : MonoBehaviour, ISelectable
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                          ◆◆◆◆◆◆ STATIC PROPERTIES ◆◆◆◆◆◆                                           ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public static readonly float FovAngleInDegrees = 30f;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private List<Unit> DeadUnits;
        private TransformAccessWrapper<Unit> UnitsListWrapper;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [field:SerializeField] public ulong OwnerID { get; private set; }
        [field: SerializeField] public int TeamID { get; private set; }
        [field:SerializeField] public int RegimentID { get; private set; }
        [field:SerializeField] public RegimentType RegimentType { get; private set; }
        
        public Formation CurrentFormation { get; private set; }
        public RegimentStateMachine StateMachine { get; private set; }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ ISelectable ◈◈◈◈◈◈                                                                             ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        [field:SerializeField] public bool IsPreselected { get; set; }
        [field:SerializeField] public bool IsSelected { get; set; }

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
            RegimentID = transform.GetInstanceID();
            name = regimentName ?? $"Player{ownerID}_Regiment{RegimentID}";
            OwnerID = ownerID;
            TeamID = teamID;
            RegimentType = regimentType;
            CurrentFormation = new Formation(regimentType, direction);
        }

        private void CreateAndRegisterUnits(UnitFactory unitFactory)
        {
            List<Unit> units = unitFactory.CreateRegimentsUnit(this, CurrentFormation.BaseNumUnits, RegimentType.UnitPrefab);
            UnitsListWrapper = new TransformAccessWrapper<Unit>(units);
            //almost impossible a regiment loose more than 20% of it's member during a frame
            DeadUnits = new List<Unit>((int)(units.Count * 0.2f));
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
            if (ClearDeadUnits())
            {
                Debug.Log($"Regiment Update: Clear Units");
            }
            StateMachine.OnUpdate();
            Units.ForEach(unit => unit.UpdateUnit());
        }

        public void OnLateUpdate()
        {
            if (!ClearDeadUnits()) return;
            Debug.Log($"Regiment LateUpdate: Clear Units");
        }

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Regiment Update Event ◇◇◇◇◇◇                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        //CAREFUL: Event received from Fixed Update => Before Update
        public void OnDeadUnit(Unit unit)
        {
            DeadUnits.AddUnique(unit);
        }

        private void ClearDeadUnits2()
        {
            for (int i = Units.Count-1; i > -1; i--)
            {
                if (!Units[i].IsDead) continue;
                UnitsListWrapper.RemoveAt(i);
                CurrentFormation.Decrement();
            }
        }
        
        private bool ClearDeadUnits()
        {
            if (DeadUnits.Count == 0) return false;
            for (int i = DeadUnits.Count - 1; i > -1; i--)
            {
                RemoveDeadUnitAt(i);
                /*
                Unit deadUnit = DeadUnits[i];
                UnitsListWrapper.Remove(deadUnit);
                DeadUnits.RemoveAt(i);
                deadUnit.OnDeath();
                CurrentFormation.Decrement();
                */
            }
            return true;
        }
        
        private void RemoveDeadUnitAt(int deadIndex)
        {
            Unit deadUnit = DeadUnits[deadIndex];
            UnitsListWrapper.Remove(deadUnit);
            DeadUnits.RemoveAt(deadIndex);
            deadUnit.OnDeath();
            CurrentFormation.Decrement();
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Rearrangement ◇◇◇◇◇◇                                                                               │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void Rearrangement()
        {
            if (DeadUnits.Count == 0) return;
            int cacheNumDead = DeadUnits.Count;
            while (DeadUnits.Count != 0)
            {
                ManualRearrange();
            }
            CurrentFormation.Remove(cacheNumDead);

            if (CurrentFormation.NumUnitsAlive == 0)
            {
                //TODO : DEAD REGIMENT
            }
            else
            {
                RegimentManager.Instance.RegimentHighlightSystem.ResizeBuffer(RegimentID, cacheNumDead);
            }
            //Need to Update NumHighlights
        }
        
        private void RemoveDeadUnit(Unit deadUnit)
        {
            UnitsListWrapper.Remove(deadUnit);
            DeadUnits.Remove(deadUnit);
            deadUnit.OnDeath();
            CurrentFormation.Decrement();
        }
        
        public void ManualRearrange()
        {
            Unit firstDeadUnit = DeadUnits[0];
            int deadIndex = firstDeadUnit.IndexInRegiment;
            int swapIndex = Units.GetIndexAround(deadIndex, CurrentFormation);
            if (swapIndex == -1)
            {
                DeadUnits.Remove(firstDeadUnit);
                return;
            }
            
            //TODO: need to replace all units in last Line
            HighlightRegister register = RegimentManager.Instance.RegimentHighlightSystem.Placement.StaticPlacementRegister;
            ReplaceStaticPlacements(register);
            
            Vector3 positionToGo = register.Records[RegimentID][deadIndex].transform.position;
            
            //Index in regiment n'est pas clair, il faut voir quand le changer!
            SwapUnitsIndex(Units[deadIndex], Units[swapIndex]);
            
            //Ici on envoie le message a l'unité de bouger!
            //rearrangementSequence.Reorganize(Units[swapIndex], positionToGo);
            
            (Units[deadIndex], Units[swapIndex]) = (Units[swapIndex], Units[deadIndex]);
            (UnitsTransform[deadIndex], UnitsTransform[swapIndex]) = (UnitsTransform[swapIndex], UnitsTransform[deadIndex]);
            
            DeadUnits.Remove(Units[deadIndex]);
            DeadUnits.Add(Units[swapIndex]);
        }

        private void SwapUnitsIndex(Unit unitA, Unit unitB)
        {
            int indexA = unitA.IndexInRegiment;
            int indexB = unitB.IndexInRegiment;
            unitA.SetIndexInRegiment(indexB);
            unitB.SetIndexInRegiment(indexA);
        }
        
        //TODO: REPLACE in corresponding register
        public void ReplaceStaticPlacements(HighlightRegister register)
        {
            FormationData tempsFormation = new(CurrentFormation,CurrentFormation.NumUnitsAlive - 1);
            
            //Formation.SetNumUnits(units.Length - 1); //Need to update formation first
            if (tempsFormation.Depth == 1 || tempsFormation.IsLastLineComplete) return;

            float unitSpace = tempsFormation.SpaceBetweenUnits;
            float3 offset = tempsFormation.GetLastLineOffset(unitSpace);
            
            int startIndex = (tempsFormation.NumCompleteLine - 1) * tempsFormation.Width;
            int firstHighlightIndex = startIndex + tempsFormation.Width;
            
            float3 startPosition = register.Records[RegimentID][startIndex].transform.position;
            startPosition -= tempsFormation.Direction3DForward * unitSpace + offset;
            
            for (int i = 0; i < tempsFormation.NumUnitsLastLine; i++)
            {
                float3 linePosition = startPosition + tempsFormation.Direction3DLine * (unitSpace * i);
                register.Records[RegimentID][firstHighlightIndex + i].transform.position = linePosition;
            }
        }
        
    }
}
