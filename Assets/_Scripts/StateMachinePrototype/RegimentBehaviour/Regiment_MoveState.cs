using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Kaizerwald.KzwMath;
using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static Unity.Mathematics.math;
using static Kaizerwald.UnityMathematicsExtension;
using static Kaizerwald.CSharpContainerUtils;

using System.Diagnostics; 
using Debug=UnityEngine.Debug;

namespace Kaizerwald
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
    //║ ◈◈◈◈◈◈ Getters ◈◈◈◈◈◈                                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        public float Speed => RegimentAttach.RegimentType.Speed;
        public float MoveSpeed => Speed * SpeedModifier;
        public bool IsRunning => SpeedModifier == RunSpeed;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Setters ◈◈◈◈◈◈                                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void SetSpeedModifier(EMoveType moveType) => SpeedModifier = moveType == EMoveType.Run ? RunSpeed : MarchSpeed;
        private int SetMarching() => SpeedModifier = MarchSpeed;
        private int SetRunning() => SpeedModifier = RunSpeed;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment_MoveState(RegimentBehaviourTree behaviourTree) : base(behaviourTree,EStates.Move)
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
            //ATTENTION entre l'ordre et la réception des pertes on pu avoir lieu(info: formation périmée!)! prendre directement sur le régiment!
            MoveOrder moveOrder = (MoveOrder)order;
            RegimentBlackboard.OnOrder(order);
            SetSpeedModifier(moveOrder.MoveType);
        }

        public override void OnEnter()
        {
            LeaderReachDestination = false;
            UpdateDestinationReach();
            
            //SOIT on check qu'on est en "chasse" ici et on assign:
            //JobAssignIndexToUnits(RegimentBlackboard.DestinationFormation);
            AssignIndexToUnits2(RegimentBlackboard.DestinationFormation);
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
    //║ ◈◈◈◈◈◈ On Enter ◈◈◈◈◈◈                                                                                         ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        private float[] GetCostMatrix(in FormationData formation)
        {
            float3 leaderDestination = RegimentBlackboard.Destination;
            float3[] destinations = formation.GetUnitsPositionRelativeToRegiment(RegimentBlackboard.Destination);
            int matrixLength = square(formation.NumUnitsAlive);
            float[] nativeCostMatrix = new float[matrixLength];

            for (int i = 0; i < matrixLength; i++)
            {
                (int x, int y) = GetXY(i, formation.NumUnitsAlive);
                float3 unitPosition = RegimentAttach.Units[y].transform.position;
                //SEMBLE FONCTIONNER! ajouter distance au leader(avec un poids moindre)
                //float distanceToLeaderDest = distance(unitPosition, RegimentBlackboard.Destination);
                float distanceToLeaderDest = distancesq(unitPosition, RegimentAttach.RegimentPosition);
                float distanceToUnitDestination = distancesq(unitPosition, destinations[x]);
                float distancePoint = distanceToUnitDestination + distanceToLeaderDest;
                nativeCostMatrix[i] = distancePoint;
            }
            return nativeCostMatrix;
        }

        private readonly Stopwatch timer = new Stopwatch();
        //public Stopwatch timer = new Stopwatch();
        //l'ALGO FONCTOIONNE! mais la matrice de cout a besoin d'être précis dans ce qu'elle demande
        //ATTENTION distance euclidienne seule, ne suffit jamais! il faut ajouter un point "d'ancrage" comme la distance par rapport au leader
        private void AssignIndexToUnits2(in FormationData formation)
        {
            //Quaternion targetRotation = Quaternion.LookRotation(formation.Direction3DForward, Vector3.up);
            float[] costMatrix = GetCostMatrix(formation);

            timer.Start();
            int[] sortedIndex = StandardHungarianAlgorithm.StandardFindAssignments2(costMatrix, formation.NumUnitsAlive);
            //int[] sortedIndex = GabiHungarianAlgorithm.FindAssignments(GetMultiCostMatrix(formation));
            timer.Stop();
            Debug.Log($"StandardFindAssignments Timer: {timer.Elapsed.TotalMilliseconds} ms");
            timer.Reset();
            
            List<Unit> tempUnitList = new List<Unit>(RegimentAttach.Units);
            for (int i = 0; i < sortedIndex.Length; i++)
            {
                int closestUnitIndex = sortedIndex[i];
                tempUnitList[i].SetIndexInRegiment(closestUnitIndex);
            }
        }
        
        private NativeArray<float> GetNativeCostMatrix(in FormationData formation)
        {
            int matrixLength = square(formation.NumUnitsAlive);
            
            float3[] destinations = formation.GetUnitsPositionRelativeToRegiment(RegimentBlackboard.Destination);
            NativeArray<float> nativeCostMatrix = new (matrixLength, TempJob, UninitializedMemory);
            
            for (int i = 0; i < matrixLength; i++)
            {
                (int x, int y) = GetXY(i, formation.NumUnitsAlive);
                float3 unitPosition = RegimentAttach.Units[y].transform.position;
                
                float distanceToLeaderDest = distancesq(unitPosition, RegimentAttach.RegimentPosition);
                float distanceToUnitDestination = distancesq(unitPosition, destinations[x]);
                float distancePoint = distanceToUnitDestination + distanceToLeaderDest;
                
                nativeCostMatrix[i] = distancePoint;
            }
            return nativeCostMatrix;
        }

        
        private void JobAssignIndexToUnits(in FormationData formation)
        {
            timer.Start();
            NativeArray<float> nativeCostMatrix = GetNativeCostMatrix(formation);
            NativeArray<int> sortedIndex = JobifiedHungarian2.FindAssignments(nativeCostMatrix, formation.NumUnitsAlive);
            List<Unit> tempUnitList = new List<Unit>(RegimentAttach.Units);
            for (int i = 0; i < sortedIndex.Length; i++)
            {
                int closestUnitIndex = sortedIndex[i];
                tempUnitList[i].SetIndexInRegiment(closestUnitIndex);
            }
            sortedIndex.Dispose();
            nativeCostMatrix.Dispose();
            timer.Stop();
            Debug.Log($"JobAssignIndexToUnits Timer: {timer.Elapsed.TotalMilliseconds} ms");
            timer.Reset();
        }


        /*
        private void AssignIndexToUnits(in FormationData formation)
        {
            NativeArray<float2> destinations = formation.GetUnitsPositionRelativeToRegiment(RegimentBlackboard.Destination.xz, Temp);
            List<Unit> tempUnitList = new List<Unit>(RegimentAttach.Units);
            for (int i = 0; i < destinations.Length; i++)
            {
                int closestUnitIndex = FindClosestUnitIndex(tempUnitList, destinations[i]);
                if (closestUnitIndex == -1) continue;
                tempUnitList[closestUnitIndex].SetIndexInRegiment(i);
                tempUnitList.RemoveAt(closestUnitIndex);
            }
        }

        private int FindClosestUnitIndex(IReadOnlyList<Unit> unitList, float2 destination)
        {
            (float closestDistance, int closestUnitIndex) = (float.MaxValue, -1);
            for (int unitIndex = 0; unitIndex < unitList.Count; unitIndex++)
            {
                float2 unitPosition = unitList[unitIndex].transform.position.xz();
                float distanceWithDst = distancesq(unitPosition, destination);
                if (distanceWithDst > closestDistance) continue;
                (closestDistance, closestUnitIndex) = (distanceWithDst, unitIndex);
            }
            return closestUnitIndex;
        }
        */
    }
}
