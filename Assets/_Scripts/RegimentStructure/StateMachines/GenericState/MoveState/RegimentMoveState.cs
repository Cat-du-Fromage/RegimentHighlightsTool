using System.Collections;
using System.Collections.Generic;
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
    public sealed class RegimentMoveState : MoveState<Regiment>
    {
        private bool LeaderReachDestination;
        public FormationData FormationDestination { get; private set; }

        public RegimentMoveState(Regiment regiment) : base(regiment, regiment.RegimentType.Speed)
        {
            Destination = ObjectTransform.position;
            FormationDestination = regiment.CurrentFormation;
        }

        public override void OnStateEnter(Order order)
        {
            ResetDefaultValues();
            MoveOrder moveOrder = (MoveOrder)order;
            
            //-------------------------------------------------------------------------------------------
            //Seems Out of Place
            ObjectAttach.CurrentFormation.SetWidth(moveOrder.FormationDestination.Width);
            ObjectAttach.CurrentFormation.SetDirection(moveOrder.FormationDestination.Direction3DForward);
            //-------------------------------------------------------------------------------------------
            
            Destination = moveOrder.LeaderDestination;
            FormationDestination = moveOrder.FormationDestination;
            AssignIndexToUnits(FormationDestination);//ICI on va donner aux unité leur index d'assignation
        }

        public override void OnStateUpdate()
        {
            //Update Leader
            //TODO: Update formation one by one (test: 1 second per Width difference)

            //SET Position and Rotation
            if (!LeaderReachDestination) // Units may still be on their way
            {
                float3 direction = normalizesafe(Destination - Position);
                float3 translation = Time.deltaTime * MoveSpeed * direction;
                ObjectTransform.Translate(translation, Space.World);
                ObjectTransform.LookAt(Position + FormationDestination.Direction3DForward);
            }
            
            if (!OnTransitionCheck()) return;
            LinkedStateMachine.TransitionDefaultState();
        }

        public override bool OnTransitionCheck()
        {
            if (!LeaderReachDestination)
            {
                LeaderReachDestination = distance(Position, Destination) <= 0.01f;
            }
            
            bool allUnitsReachDestination = true;
            foreach (UnitStateMachine unitStateMachine in ObjectAttach.StateMachine.UnitsStateMachine)
            {
                if (unitStateMachine.IsIdle) continue;
                allUnitsReachDestination = false;
                break;
            }
            return allUnitsReachDestination && LeaderReachDestination;
        }

        public override void OnStateExit()
        {
            ResetDefaultValues();
        }

        public override void ResetDefaultValues()
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
            List<Unit> tempUnitList = new(ObjectAttach.Units);
            
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
    }
}

/*
public int[] GetIndicesOrderedByFurthestLineThenMiddle(FormationData formation)
{
    //Concept: on obtient les index dont on souhaite vérifier d'abord la distance
    int[] orderedIndices = new int[formation.NumUnitsAlive];
    int realIndex = 0;
    
    for (int y = 0; y < formation.Depth; y++)
    {
        // ATTENTION DERNIERE LIGNE qui peut être plus petite!!!!
        int width = y == formation.Depth - 1 ? formation.NumUnitsLastLine : formation.Width;
        int start = width / 2; 
        int sign = 1;
        for (int i = 0; i < width; i++)
        {
            int x = start + (int)ceil(i / 2f) * sign;
            sign *= -1;
            int index = y * formation.Width + x;
            orderedIndices[realIndex++] = index;
        }
    }
    return orderedIndices; 
}

public void AssignIndexToUnits(FormationData formation)
{
    Vector2[] destinations = formation.GetUnitsPositionRelativeToRegiment(Destination);
    List<Unit> tempUnitList = new List<Unit>(ObjectAttach.Units);
    //List<Unit> tempUnitList = new (ObjectAttach.Units.Count);
    //tempUnitList.AddRange(ObjectAttach.Units);

    SortedSet<KeyValuePair<int, float>> distances = new(CSharpContainerUtils.GetKeyValuePairComparer<int, float>());
    for(int i = 0; i < destinations.Length; i++)
    {
        float2 currentDestinationCheck = destinations[i];
        GatherUnitsDistance();
        SetUnitIndex();
        distances.Clear();
        
        void GatherUnitsDistance()
        {
            for(int unitIndex = 0; unitIndex < tempUnitList.Count; unitIndex++)
            {
                float3 unitPosition = tempUnitList[unitIndex].transform.position;
                float distanceWithDst = distancesq(unitPosition.xz, currentDestinationCheck);
                KeyValuePair<int, float> pair = new (unitIndex, distanceWithDst);
                distances.Add(pair);
            }
        }
        
        void SetUnitIndex()
        {
            Unit unitToRemove = tempUnitList[distances.Min.Key];
            unitToRemove.SetIndexInRegiment(i);
            tempUnitList.Remove(unitToRemove);
        }  
    }
}

*/
