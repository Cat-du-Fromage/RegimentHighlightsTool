using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static KaizerWald.KzwMath;
using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static Unity.Mathematics.math;
using static KaizerWald.UnityMathematicsExtension;
using static KaizerWald.CSharpContainerUtils;

namespace KaizerWald
{
    public sealed class Regiment_MoveState : RegimentStateBase
    {
        private readonly int MarchSpeed;
        private readonly int RunSpeed;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public bool LeaderReachDestination { get; private set; }
        public int SpeedModifier { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Getters ◈◈◈◈◈◈                                                                                     ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        public float Speed => RegimentAttach.RegimentType.Speed;
        public float MoveSpeed => Speed * SpeedModifier;
        public bool IsRunning => SpeedModifier == RunSpeed;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Setters ◈◈◈◈◈◈                                                                                     ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void SetSpeedModifier(EMoveType moveType) => SpeedModifier = moveType == EMoveType.Run ? RunSpeed : MarchSpeed;
        private int SetMarching() => SpeedModifier = MarchSpeed;
        private int SetRunning() => SpeedModifier = RunSpeed;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment_MoveState(RegimentBehaviourTree behaviourTree, Blackboard blackBoard) : base(behaviourTree,blackBoard,EStates.Move)
        {
            MarchSpeed = 1;
            RunSpeed = 3;
        }

        public override bool ConditionEnter()
        {
            return base.ConditionEnter();
        }

        public override void OnSetup(Order order)
        {
            MoveOrder moveOrder = (MoveOrder)order;
            RegimentBlackboard.OnOrder(order);
            SetSpeedModifier(moveOrder.MoveType);
            
            //UPDATE PLACEMENT HERE??
            
        }

        public override void OnEnter()
        {
            LeaderReachDestination = false;
            UpdateDestinationReach();
            
            //SOIT on check qu'on est en "chasse" ici et on assign:
            //RegimentBlackboard.SetDestination(moveOrder.LeaderDestination);
            //RegimentBlackboard.SetDestinationFormation(moveOrder.FormationDestination);
            
            //SOIT il faut revoir comment l'ordre est donné
            //vérifier distance etc..
            
            AssignIndexToUnits(RegimentBlackboard.DestinationFormation);
            RegimentAttach.CurrentFormation.SetWidth(RegimentBlackboard.DestinationFormation.Width);
            RegimentAttach.CurrentFormation.SetDirection(RegimentBlackboard.DestinationFormation.Direction3DForward);
        }

        public override void OnUpdate()
        {
            if (LeaderReachDestination) return;
            MoveRegiment();
        }

        public override void OnExit()
        {
            //
        }

        public override EStates ShouldExit()
        {
            UpdateDestinationReach();
            if (!LeaderReachDestination) return StateIdentity;
            return RegimentBlackboard.IsChasing ? EStates.Fire : EStates.Idle;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void UpdateDestinationReach()
        {
            if (LeaderReachDestination) return;
            LeaderReachDestination = distance(Position, RegimentBlackboard.Destination) <= 0.01f;
        }

        private void MoveRegiment()
        {
            if (LeaderReachDestination) return; // Units may still be on their way
            float3 direction = normalizesafe(RegimentBlackboard.Destination - Position);
            float3 translation = Time.deltaTime * MoveSpeed * direction;
            BehaviourTree.CachedTransform.Translate(translation, Space.World);
            BehaviourTree.CachedTransform.LookAt(Position + RegimentBlackboard.DestinationFormation.Direction3DForward);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ On Enter ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        private void AssignIndexToUnits(in FormationData formation)
        {
            NativeArray<float2> destinations = formation.GetUnitsPositionRelativeToRegiment(RegimentBlackboard.Destination.xz);
            List<Unit> tempUnitList = new List<Unit>(RegimentAttach.Units);

            for (int i = 0; i < destinations.Length; i++)
            {
                int closestUnitIndex = FindClosestUnitIndex(tempUnitList, destinations[i]);
                if (closestUnitIndex == -1) continue;
                
                Unit closestUnit = tempUnitList[closestUnitIndex];
                closestUnit.SetIndexInRegiment(i);
                tempUnitList.RemoveAt(closestUnitIndex);
            }
        }

        private int FindClosestUnitIndex(List<Unit> unitList, float2 destination)
        {
            (float closestDistance, int closestUnitIndex) = (float.MaxValue, -1);
            //int closestUnitIndex = -1;
            for (int unitIndex = 0; unitIndex < unitList.Count; unitIndex++)
            {
                float3 unitPosition = unitList[unitIndex].transform.position;
                float distanceWithDst = distancesq(unitPosition.xz, destination);

                if (distanceWithDst > closestDistance) continue;
                closestDistance = distanceWithDst;
                closestUnitIndex = unitIndex;
            }
            return closestUnitIndex;
        }
    
    /*
        private void AssignIndexToUnits(in FormationData formation)
        {
            //Test(formation);
            NativeArray<float2> destinations = formation.GetUnitsPositionRelativeToRegiment(RegimentBlackboard.Destination.xz);
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
        */
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

        private void Test(FormationData formation)
        {
            int regimentNumUnits = RegimentAttach.Units.Count;
            
            using NativeArray<float2> destinations = formation.GetUnitsPositionRelativeToRegiment(RegimentBlackboard.Destination.xz);
            /*
            using NativeArray<KeyValuePair<int, float>> distances = new (Square(regimentNumUnits), TempJob, UninitializedMemory);

            //FIRST Bloc
            CustomComparer comparerKeyValue = new CustomComparer();
            NativeArray<JobHandle> jobHandles= new(regimentNumUnits, Temp, UninitializedMemory);
            for (int unitIndex = 0; unitIndex < regimentNumUnits; unitIndex++)
            {
                JTestGatherDistances jGatherDistance = new JTestGatherDistances
                {
                    ComparerKeyValue = comparerKeyValue,
                    NumUnits = RegimentAttach.Units.Count,
                    Unit2DPosition = RegimentAttach.Units[unitIndex].transform.position.xz(),
                    Destinations = destinations,
                    Distances = distances
                };
                //JobHandle dependency = unitIndex == 0 ? default : jobhandles
                jobHandles[unitIndex] = jGatherDistance.ScheduleParallel(regimentNumUnits, JobWorkerCount-1, default);
            }
            JobHandle combinedDependency = JobHandle.CombineDependencies(jobHandles);
            combinedDependency.Complete();
            */
            //SECOND Bloc
            //Sort closer from regiment destination
            SortedSet<KeyValuePair<int, float>> distancePairUnitIndex = new (GetKeyValuePairComparer<int, float>());
            GatherUnitsDistance(distancePairUnitIndex, RegimentAttach.Units, RegimentBlackboard.Destination.xz);
            using NativeArray<int> indexReverseSorted = GetIndexSortedByFurthest(distancePairUnitIndex);
            
            //Ici on va chercher depuis la destinations la plus éloignées, un candidat
            HashSet<Unit> tmpUnits = new HashSet<Unit>(RegimentAttach.Units);
            for (int i = 0; i < indexReverseSorted.Length; i++)
            {
                int placementDestinationIndex = indexReverseSorted[i]; // a attribuer

                Unit unitToSet = tmpUnits.First();
                float minDistance = distancesq(destinations[placementDestinationIndex], unitToSet.transform.position.xz());
                //On va chercher dans chaque unité à la clé "destinationIndex" la valeur la plus petite
                foreach (Unit unit in tmpUnits)
                {
                    //if (indicesAttributed.Contains(unit.IndexInRegiment)) continue;
                    float distanceCalcul = distancesq(destinations[placementDestinationIndex], unit.transform.position.xz());
                    if(distanceCalcul > minDistance) continue;
                    unitToSet = unit;
                    minDistance = distanceCalcul;
                }
                unitToSet.SetIndexInRegiment(placementDestinationIndex);
                tmpUnits.Remove(unitToSet);
                //unitToRemove.SetIndexInRegiment(i);
            }
        }
        
        [BurstCompile]
        public struct CustomComparer : IComparer<KeyValuePair<int, float>>
        {
            public int Compare(KeyValuePair<int, float> x, KeyValuePair<int, float> y)
            {
                return x.Value.CompareTo(y.Value);
            }
        }

        [BurstCompile]
        private struct JTestGatherDistances : IJobFor
        {
            [ReadOnly] public CustomComparer ComparerKeyValue;
            [ReadOnly] public int NumUnits;
            [ReadOnly] public float2 Unit2DPosition;
            [ReadOnly] public NativeArray<float2> Destinations;
            [WriteOnly] public NativeArray<KeyValuePair<int, float>> Distances;
            
            public void Execute(int index)
            {
                int startIndex = index * NumUnits;
                for (int i = 0; i < Destinations.Length; i++)
                {
                    Distances[startIndex + i] = new KeyValuePair<int, float>(i, distancesq(Unit2DPosition, Destinations[i]));
                }
                NativeSlice<KeyValuePair<int, float>> tmp = Distances.Slice(startIndex, startIndex + NumUnits);
                tmp.Sort(ComparerKeyValue);
            }
        }

        private NativeArray<int> GetIndexSortedByFurthest(SortedSet<KeyValuePair<int, float>> distances)
        {
            NativeArray<int> indexSorted = new (distances.Count, TempJob, UninitializedMemory);
            int index = 0;
            foreach (KeyValuePair<int, float> pair in distances.Reverse())
            {
                indexSorted[index++] = pair.Key;
            }
            return indexSorted;
        }
    }
}
