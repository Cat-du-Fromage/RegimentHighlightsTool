using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Jobs;

using static UnityEngine.Quaternion;
using static Unity.Mathematics.math;

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
        private Transform regimentTransform;
        //private List<Unit> DeadUnits;
        private SortedSet<int> DeadUnits;
        public RegimentFormationMatrix RegimentFormationMatrix { get; private set; }
        
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

        public float3 RegimentPosition => regimentTransform.position;
        public List<Unit> Units => RegimentFormationMatrix.Units;
        public List<Transform> UnitsTransform => RegimentFormationMatrix.Transforms;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            regimentTransform = transform;
        }

        private void OnDestroy()
        {
            
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Regiment Update Event ◈◈◈◈◈◈                                                                   ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnUpdate()
        {
            Rearrangement();
            StateMachine.OnUpdate();
            Units.ForEach(unit => unit.UpdateUnit());
        }

        public void OnLateUpdate()
        {
            //if (!ClearDeadUnits()) return;
            //Debug.Log($"Regiment LateUpdate: Clear Units");
        }
        
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
            RegimentFormationMatrix = new RegimentFormationMatrix(units);
            //almost impossible a regiment loose more than 20% of it's member during a frame
            DeadUnits = new SortedSet<int>();
        }

        private void InitializeStateMachine()
        {
            StateMachine = this.GetOrAddComponent<RegimentStateMachine>();
            StateMachine.Initialize();
        }

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Regiment Update Event ◇◇◇◇◇◇                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        //CAREFUL: Event received from Fixed Update => Before Update
        public void OnDeadUnit(Unit unit)
        {
            DeadUnits.Add(unit.IndexInRegiment);
            // -------------------------------------------------------
            // Seems Wrong place but not better Idea for State Machine...
            StateMachine.DeadUnitsStateMachine.Add(unit.StateMachine);
            // -------------------------------------------------------
        }

        private bool ConfirmDeaths(out int numDead)
        {
            numDead = DeadUnits.Count;
            if (DeadUnits.Count == 0) return false;
            foreach (int indexInRegiment in DeadUnits)
            {
                Units[indexInRegiment].ConfirmDeathByRegiment(this);
            }
            return true;
        }



        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Rearrangement ◇◇◇◇◇◇                                                                               │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        //TODO COMMENT ON TROUVE LA POSITION! car les enemy n'ont pas de "placement" => pas de rearrangement possible en utilisant les jetons!
        
        private void Rearrangement()
        {
            if (!ConfirmDeaths(out int cacheNumDead)) return;
            
            FormationData futureFormation = new (CurrentFormation,  CurrentFormation.NumUnitsAlive - cacheNumDead);
            float3 regimentPosition = !StateMachine.IsMoving ? RegimentPosition : ((RegimentMoveState)StateMachine.CurrentState).Destination;
            MoveOrder moveOrder = new MoveOrder(futureFormation, regimentPosition);
            
            while (DeadUnits.Count != 0)
            {
                Rearrange(moveOrder, DeadUnits.Min, futureFormation);
            }
            LastLineRearrangementOrder(moveOrder, futureFormation);
            ResizeFormation(cacheNumDead);
            
            if (CurrentFormation.NumUnitsAlive == 0) return;
            //ATTENTION! SEUL LE JOUEUR A DES PLACEMENTS ET SELECTION
            RegimentManager.Instance.RegimentHighlightSystem.ResizeHighlightsRegisters(this, regimentPosition);
        }
        
        private void ResizeFormation(int numToRemove)
        {
            RegimentFormationMatrix.Resize(numToRemove);
            CurrentFormation.DecreaseBy(numToRemove);
        }

        private void LastLineRearrangementOrder(Order order, in FormationData futureFormation)
        {
            if (futureFormation.IsLastLineComplete) return;
            int lastRowFirstIndex = futureFormation.NumUnitsAlive - futureFormation.NumUnitsLastLine;
            for (int i = lastRowFirstIndex; i < futureFormation.NumUnitsAlive; i++)
            {
                //MoveOrder unitMoveOrder = new MoveOrder(futureFormation, regimentPosition);
                Units[i].StateMachine.ChangeState(order);
            }
        }

        private void Rearrange(Order order, int deadIndex, in FormationData futureFormation)
        {
            //Here we use Current Formation because units are not consider "Dead" Yet
            int swapIndex = FormationUtils.GetIndexAround(Units, deadIndex, CurrentFormation);
            if (swapIndex == -1)
            {
                DeadUnits.Remove(deadIndex);
                return;
            }
            Unit unitToSwapWith = Units[swapIndex];
            
            // ----------------------------------------------------------------------
            // (1) we need the position "unitToSwapWith" will go
            // we can't use dead unit as reference because:
            // * it's unstable because of the death animation
            // * Very Unstable when applying to Last Line because with vary a lot
            // ==> We need to calculate position in regiment (using formationExtension?)
            // ----------------------------------------------------------------------
            RegimentFormationMatrix.SwapIndexInRegiment(deadIndex, swapIndex);
            
            DeadUnits.Remove(deadIndex);
            DeadUnits.Add(swapIndex);
            
            int deadYCoord = deadIndex / futureFormation.Width;
            int swappedYCoord = swapIndex / futureFormation.Width;
            bool2 areUnitsOnLastLine = new (deadYCoord == futureFormation.Depth - 1, deadYCoord == swappedYCoord);
            if(all(areUnitsOnLastLine)) return;
            
            //MoveOrder unitMoveOrder = new MoveOrder(futureFormation, regimentPosition);
            unitToSwapWith.StateMachine.ChangeState(order);
        }
    }
}
