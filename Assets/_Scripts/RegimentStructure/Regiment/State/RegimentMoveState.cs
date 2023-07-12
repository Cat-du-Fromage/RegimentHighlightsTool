using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem.Controls;
using static Unity.Mathematics.math;

namespace KaizerWald
{
    public sealed class RegimentMoveState : MoveState<Regiment>
    {
        private bool LeaderReachDestination;
        public Formation FormationDestination { get; private set; }
        private float2 Destination2D => ((float3)Destination).xz;
        
        public RegimentMoveState(Regiment regiment) : base(regiment, regiment.RegimentType.Speed)
        {
            Destination = ObjTransform.position;
            FormationDestination = regiment.CurrentFormation;
        }

        public override void OnStateEnter()
        {
            LeaderReachDestination = false;
            SpeedModifier = 1;
            ObjectAttach.CurrentFormation.SetWidth(FormationDestination.Width);
        }
        
        public override void OnOrderEnter(RegimentOrder order)
        {
            MoveRegimentOrder moveOrder = (MoveRegimentOrder)order;
            Destination = moveOrder.LeaderDestination;
            FormationDestination = moveOrder.FormationDestination;
            
            AssignIndexToUnits(moveOrder.FormationDestination);
            //ICI on va donner aux unité leur index d'assignation
        }

        public override void OnStateUpdate()
        {
            //Update Leader
            //TODO: Update formation one by one

            //SET Position and Rotation
            if (!LeaderReachDestination) // Units may still be on their way
            {
                Vector3 direction = (Destination - Position).normalized;
                Vector3 translation = Time.deltaTime * MoveSpeed * direction;
                ObjTransform.Translate(translation, Space.World);
                ObjTransform.LookAt(Position + (Vector3)FormationDestination.DirectionForward);
            }
            
            if (!OnTransitionCheck()) return;
            ObjectAttach.StateMachine.TransitionState(EStates.Idle);
        }

        public override bool OnTransitionCheck()
        {
            if (!LeaderReachDestination)
            {
                LeaderReachDestination = (Destination - Position).magnitude <= 0.01f;
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
            
        }
        
        public void AssignIndexToUnits(FormationData formation)
        {
            Vector2[] destinations = formation.GetUnitsPositionRelativeToRegiment(Destination);
            
            List<Unit> tempUnitList = new (ObjectAttach.Units.Count);
            tempUnitList.AddRange(ObjectAttach.Units);
            
            var comparer = Comparer<KeyValuePair<int, float>>.Create((a, b) => a.Value.CompareTo(b.Value));
            SortedSet<KeyValuePair<int, float>> distances = new(comparer);
            
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
                        KeyValuePair<int, float> pair = new KeyValuePair<int, float>(unitIndex, distanceWithDst);
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
*/
