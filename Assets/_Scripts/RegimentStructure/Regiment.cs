using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;

using static UnityEngine.Quaternion;
using static Unity.Mathematics.math;
using static Kaizerwald.FormationUtils;

namespace Kaizerwald
{
    public partial class Regiment : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                          ◆◆◆◆◆◆ STATIC PROPERTIES ◆◆◆◆◆◆                                           ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public static readonly float FovAngleInDegrees = 30f;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Transform RegimentTransform { get; private set; }
        
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
        public RegimentBehaviourTree BehaviourTree { get; private set; }
        
        public HighlightRegiment Highlight { get; private set; }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        public float3 RegimentPosition => RegimentTransform.position;
        public List<Unit> Units => RegimentFormationMatrix.Units;
        public List<Transform> UnitsTransform => RegimentFormationMatrix.Transforms;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            RegimentTransform = transform;
        }

        private void Start()
        {
            Highlight = GetComponent<HighlightRegiment>();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Regiment Update Event ◈◈◈◈◈◈                                                                            ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void OnFixedUpdate()
        {
            //Seems there is less buggy by making rearrangement strictly before Update
            //Maybe because it's on the same update than bullet collision?
            Rearrangement();
        }

        public void OnUpdate()
        {
            BehaviourTree.OnUpdate();
            Units.ForEach(unit => unit.UpdateUnit());
        }

        public void OnLateUpdate()
        {
            return;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        public void Initialize(ulong ownerID, int teamID, UnitFactory unitFactory, RegimentSpawner currentSpawner, Vector3 direction, string regimentName = default)
        {
            InitializeProperties(ownerID, teamID, currentSpawner.RegimentType, direction, regimentName);
            CreateAndRegisterUnits(unitFactory);
            RegimentManager.Instance.RegisterRegiment(this);
            
            HighlightInitialization(ownerID, teamID, currentSpawner.RegimentType, direction);
            BehaviourTree = this.GetOrAddComponent<RegimentBehaviourTree>();
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

        private void HighlightInitialization(ulong ownerID, int teamID, RegimentType regimentType, float3 direction)
        {
            RegimentClass regimentClass = regimentType.RegimentClass;
            int2 minMaxRow = new int2(regimentClass.MinRow, regimentClass.MaxRow);
            float2 unitSize = new float2(regimentClass.Category.UnitSize.x, regimentClass.Category.UnitSize.z);
            float spaceBetweenUnits = regimentClass.SpaceBetweenUnits;
            HighlightRegimentManager.Instance.RegisterRegiment(ownerID, teamID, gameObject, Units, minMaxRow, unitSize, spaceBetweenUnits, direction);
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
            BehaviourTree.DeadUnitsBehaviourTrees.Add(unit.BehaviourTree);
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
        //Problématique: ne peut pas être une state car il y a un changement de structure de la matrice de formation
        //Changement des "IndexInRegiment" des unités qui entrainerait des sauts entre les classes "StateMachine" et "Regiment"
        //Conclusion: Rearrangement n'est PAS une STATE mais une FONCTION du régiment!
        private void Rearrangement()
        {
            if (!ConfirmDeaths(out int cacheNumDead)) return;
            float3 regimentPosition = !BehaviourTree.IsMoving ? RegimentPosition : BehaviourTree.RegimentBlackboard.Destination;
            Rearrange(cacheNumDead, regimentPosition);
            ResizeFormation(cacheNumDead);
            
            if (CurrentFormation.NumUnitsAlive == 0) return;
            //ATTENTION! SEUL LE JOUEUR A DES PLACEMENTS ET SELECTION
            HighlightRegiment highlightRegiment = GetComponent<HighlightRegiment>();
            HighlightRegimentManager.Instance.ResizeHighlightsRegisters(highlightRegiment, regimentPosition);
        }
        
        private void ResizeFormation(int numToRemove)
        {
            RegimentFormationMatrix.Resize(numToRemove);
            CurrentFormation.DecreaseBy(numToRemove);
        }

        private void Rearrange(int cacheNumDead, in float3 regimentPosition)
        {
            //CAREFULL IF IN MOVEMENT
            FormationData currentFormationData = BehaviourTree.IsMoving ? BehaviourTree.RegimentBlackboard.DestinationFormation : CurrentFormation;
            FormationData futureFormation = new (currentFormationData,  currentFormationData.NumUnitsAlive - cacheNumDead);
            MoveOrder order = new MoveOrder(futureFormation, regimentPosition);
            
            while (DeadUnits.Count > 0)
            {
                SwapRearrange(currentFormationData, futureFormation, order);
            }
            LastLineRearrangementOrder(futureFormation, order);
        }
        
        private void SwapRearrange(in FormationData currentFormationData, in FormationData futureFormation, Order order)
        {
            int deadIndex = DeadUnits.Min;
            //Here we use Current Formation because units are not consider "Dead" Yet
            int swapIndex = FormationUtils.GetIndexAround(Units, deadIndex, currentFormationData);
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
            SwapDead(deadIndex, swapIndex);
            
            (int deadYCoord, int swappedYCoord) = (deadIndex / futureFormation.Width, swapIndex / futureFormation.Width);
            //bool deadUnitOnLastLine = deadYCoord == futureFormation.Depth - 1 && deadYCoord == swappedYCoord;
            //pourquoi avoir passé par un bool2?
            bool2 deadUnitOnLastLine = new bool2(deadYCoord == futureFormation.Depth - 1, deadYCoord == swappedYCoord);
            if (all(deadUnitOnLastLine)) return;
            
            unitToSwapWith.BehaviourTree.RequestChangeState(order);
        }

        private void SwapDead(int deadIndex, int swapIndex)
        {
            DeadUnits.Remove(deadIndex);
            DeadUnits.Add(swapIndex);
        }

        private void LastLineRearrangementOrder(in FormationData futureFormation, Order order)
        {
            if (futureFormation.IsLastLineComplete || futureFormation.Depth == 1) return;
            for (int i = futureFormation.LastRowFirstIndex; i < futureFormation.NumUnitsAlive; i++)
            {
                if (i >= Units.Count) continue;
                Units[i].BehaviourTree.RequestChangeState(order);
            }
        }
    }
}
