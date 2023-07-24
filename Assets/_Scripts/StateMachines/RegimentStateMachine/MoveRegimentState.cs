using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using static Unity.Mathematics.math;

using static KaizerWald.CSharpContainerUtils;

namespace KaizerWald
{
    public class MoveRegimentState : RegimentState
    {
        private const int MarchSpeed = 1;
        private const int RunSpeed = 3;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public float3 Destination { get; private set; }
        public FormationData FormationDestination { get; private set; }
        public bool LeaderReachDestination { get; private set; }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Move Related ◈◈◈◈◈◈                                                                            ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private float Speed => RegimentAttach.RegimentType.Speed;
        public int SpeedModifier { get; private set; }
        private float MoveSpeed => Speed * SpeedModifier;
        public bool IsRunning => SpeedModifier == RunSpeed;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public MoveRegimentState(RegimentStateMachine regimentStateMachine) : base(regimentStateMachine, EStates.Move)
        {
            
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void InitializeValues()
        {
            LeaderReachDestination = false;
            SetMarching();
        }
        
        private void SetMarching()
        {
            SpeedModifier = MarchSpeed;
        }
        
        private void SetRunning()
        {
            SpeedModifier = RunSpeed;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ State Related ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public override void OnAbilityTrigger()
        {
            if (IsRunning)
                SetMarching();
            else
                SetRunning();
        }
        
        public override void SetupState(Order order)
        {
            InitializeValues();
            MoveOrder moveOrder = (MoveOrder)order;
            Destination = moveOrder.LeaderDestination;
            FormationDestination = moveOrder.FormationDestination;
        }

        public override void EnterState()
        {
            //FormationDestination est aussi envoyé a Unit => Unit pourra calculer sans problème son setup
            //-------------------------------------------------------------------------------------------
            AssignIndexToUnits(FormationDestination);
            
            //Seems Out of Place
            //Dans une version future, le changement ne sera pas instant mais aura un délai(1s) pour chaque unité en +/- de la Width
            RegimentAttach.CurrentFormation.SetWidth(FormationDestination.Width);
            RegimentAttach.CurrentFormation.SetDirection(FormationDestination.Direction3DForward);
            //-------------------------------------------------------------------------------------------
        }

        public override void UpdateState()
        {
            MoveRegiment();
            CheckSwitchState();
        }
        
        public override bool CheckSwitchState()
        {
            return SwitchToIdleState();
        }
        
        public override void ExitState()
        {
            ResetValuesToDefaults();
        }
        
        public override void ResetValuesToDefaults()
        {
            LeaderReachDestination = false;
            SetMarching();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void AssignIndexToUnits(in FormationData formation)
        {
            NativeArray<float2> destinations = formation.GetUnitsPositionRelativeToRegiment(Destination.xz);
            List<Unit> tempUnitList = new(RegimentAttach.Units);
            
            SortedSet<KeyValuePair<int, float>> distances = new(GetKeyValuePairComparer<int, float>());
            for(int i = 0; i < destinations.Length; i++)
            {
                GatherUnitsDistance(distances, tempUnitList, destinations[i]);
                Unit unitToRemove = tempUnitList[distances.Min.Key];
                unitToRemove.SetIndexInRegiment(i);
                tempUnitList.Remove(unitToRemove);
                distances.Clear();
            }
        }
        
        private void GatherUnitsDistance(ISet<KeyValuePair<int, float>> distances, IReadOnlyList<Unit> unitList, float2 destination)
        {
            for(int unitIndex = 0; unitIndex < unitList.Count; unitIndex++)
            {
                float3 unitPosition = unitList[unitIndex].transform.position;
                float distanceWithDst = distancesq(unitPosition.xz, destination);
                KeyValuePair<int, float> pair = new (unitIndex, distanceWithDst);
                distances.Add(pair);
            }
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ State Related ◈◈◈◈◈◈                                                                           ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        // temporaire, pas un vrai pathfinding....
        private void MoveRegiment()
        {
            if (LeaderReachDestination) return; // Units may still be on their way
            float3 direction = normalizesafe(Destination - Position);
            float3 translation = Time.deltaTime * MoveSpeed * direction;
            RegimentTransform.Translate(translation, Space.World);
            RegimentTransform.LookAt(Position + FormationDestination.Direction3DForward);
        }

        private bool SwitchToIdleState()
        {
            LeaderReachDestination = LeaderReachDestination || distance(Position, Destination) <= 0.01f;
            if (!LeaderReachDestination) return false;
            foreach (UnitStateMachine unitStateMachine in LinkedRegimentStateMachine.UnitsStateMachine)
            {
                if (!unitStateMachine.IsIdle) return false;
            }
            LinkedRegimentStateMachine.ToDefaultState();
            return true;
        }
    }
}
