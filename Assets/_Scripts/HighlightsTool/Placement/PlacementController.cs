using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static PlayerControls;
using static UnityEngine.InputSystem.InputAction;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;
using static Unity.Mathematics.quaternion;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;

using float3 = Unity.Mathematics.float3;

namespace KaizerWald
{
    public sealed class PlacementController : HighlightController
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private const float DISTANCE_BETWEEN_REGIMENT = 1f;
        private const int ORIGIN_HEIGHT = 1000;
        private const int RAY_DISTANCE = 2000;

        private readonly LayerMask TerrainLayer;
        
        private PlacementActions PlacementControls;
        
        private bool PlacementsVisible;
        private bool MouseStartValid;
        private Vector3 MouseStart, MouseEnd;
        
        private bool PlacementCancel;
        
        private float mouseDistance;
        private int[] tempWidths;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public PlacementSystem PlacementSystem { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Getters ◈◈◈◈◈◈                                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public int[] DynamicsTempWidth => tempWidths;
    
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        private List<HighlightRegiment> SelectedRegiments => PlacementSystem.SelectedRegiments;
        private int NumSelections => PlacementSystem.SelectedRegiments.Count;
        
        private HighlightRegister DynamicRegister => PlacementSystem.DynamicPlacementRegister;
        private HighlightRegister StaticRegister => PlacementSystem.StaticPlacementRegister;
        
        private float UpdateMouseDistance() => Magnitude(MouseEnd - MouseStart);
        private float3 LineDirection => normalizesafe(MouseEnd - MouseStart);
        private float3 DepthDirection => normalizesafe(cross(up(), LineDirection));
        private int TotalUnitsSelected => SelectionInfo.GetTotalUnitsSelected(SelectedRegiments);
        private float2 MinMaxSelectionWidth => SelectionInfo.GetMinMaxSelectionWidth(SelectedRegiments) + DISTANCE_BETWEEN_REGIMENT * (NumSelections-1);
        private NativeArray<int> MinWidthsArray => SelectionInfo.GetSelectionsMinWidth(SelectedRegiments);
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public PlacementController(HighlightSystem system, PlayerControls controls, LayerMask terrainLayer) : base()
        {
            PlacementSystem = (PlacementSystem)system;
            PlacementControls = controls.Placement;
            TerrainLayer = terrainLayer;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                       ◆◆◆◆◆◆ ABSTRACT METHODS ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public override void OnEnable()
        {
            PlacementControls.Enable();
            PlacementControls.RightMouseClickAndMove.started   += OnRightMouseClickAndMoveStart;
            PlacementControls.RightMouseClickAndMove.performed += OnRightMouseClickAndMovePerform;
            PlacementControls.RightMouseClickAndMove.canceled  += OnRightMouseClickAndMoveCancel;

            PlacementControls.SpaceKey.started  += OnSpaceKeyStart;
            PlacementControls.SpaceKey.canceled += OnSpaceKeyCancel;

            PlacementControls.LeftMouseCancel.started += CancelPlacement;
        }

        public override void OnDisable()
        {
            PlacementControls.RightMouseClickAndMove.started   -= OnRightMouseClickAndMoveStart;
            PlacementControls.RightMouseClickAndMove.performed -= OnRightMouseClickAndMovePerform;
            PlacementControls.RightMouseClickAndMove.canceled  -= OnRightMouseClickAndMoveCancel;
            
            PlacementControls.SpaceKey.started  -= OnSpaceKeyStart;
            PlacementControls.SpaceKey.canceled -= OnSpaceKeyCancel;
            
            PlacementControls.LeftMouseCancel.started -= CancelPlacement;
            PlacementControls.Disable();
        }

        public override void OnUpdate()
        {
            OnCameraMoveFormation();
        }

        /// <summary>
        /// Trigger formation on camera movement
        /// </summary>
        private void OnCameraMoveFormation()
        {
            if (!PlacementsVisible) return;
            Vector3 lastPosition = MouseEnd;
            if (!UpdateMouseEnd(Mouse.current.position.value)) return;
            if (MouseEnd == lastPosition) return;
            mouseDistance = UpdateMouseDistance();
            PlaceRegiments();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                     ◆◆◆◆◆◆ EVENT BASED CONTROLS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void OnRightMouseClickAndMoveStart(CallbackContext context)
        {
            if (SelectedRegiments.Count is 0) return;
            PlacementCancel = false;
            MouseStartValid = UpdateMouseStart(context.ReadValue<Vector2>());
        }

        private void OnRightMouseClickAndMovePerform(CallbackContext context)
        {
            if (SelectedRegiments.Count is 0 || !MouseStartValid || !UpdateMouseEnd(context.ReadValue<Vector2>())) return;
            mouseDistance = UpdateMouseDistance();
            tempWidths = PlaceRegiments();
        }
        /*
        private void OnAttackCallback()
        {
            //Prevent order attack while trying to order place
            //if (PlacementsVisible || PlacementSystem.PreselectedRegiments.Count != 1) return;
            // =============================================================================
            // TODO: FOR NOW TEAM ID IS HARDCODED (not 0 means not the player) => Change When "PlayersManager"/"TeamsManager" implemented
            // =============================================================================
            Regiment preselectedRegiment = PlacementSystem.PreselectedRegiments[0];
            if (preselectedRegiment.TeamID == 0) return;
            PlacementSystem.OnAttackOrderEvent(preselectedRegiment);
        }
*/
        private void OnMoveCallback(int registerIndex)
        {
            PlacementSystem.OnMoveOrderEvent(registerIndex, tempWidths);
        }

        private void OrdersCallbackChoice(int registerIndex)
        {
            if (!PlacementsVisible)
            {
                if (PlacementSystem.PreselectedRegiments.Count == 0)
                {
                    //TODO : Implement Move while keeping same Formation(Width)
                    //Carefull WAY More complicated than it looks.. How will it work when multiple selection?
                }
                else
                {
                    //OnAttackCallback();
                }
            }
            else
            {
                OnMoveCallback(registerIndex);
            }
        }
        
        private void OnRightMouseClickAndMoveCancel(CallbackContext context)
        {
            if (SelectedRegiments.Count is 0 || PlacementCancel) return; //Means Left Click is pressed
            //Currently only drag order work
            
            //Precise Register because in this case, static are not in position, can't move after because
            //DisablePlacements methods make "PlacementsVisible = false;"
            OrdersCallbackChoice(PlacementSystem.DynamicRegisterIndex);
            OnMouseReleased();
            
            //Currently only drag order work
            //OrdersCallbackChoice();
            //PlacementSystem.OnMoveOrderEvent(tempWidths);

            void OnMouseReleased()
            {
                /*
                // Update Regiments Selected Width (remove after rework)
                for (int i = 0; i < SelectedRegiments.Count; i++) SelectedRegiments[i].CurrentFormation.SetWidth(tempWidths[i]);
                */
                DisablePlacements();
                PlacementSystem.SwapDynamicToStatic();
            }
        }

        private void OnSpaceKeyStart(CallbackContext context) => EnableAllStatic();
        private void OnSpaceKeyCancel(CallbackContext context) => DisableAllStatic();

        //Cancel Placement
        private void CancelPlacement(CallbackContext context) => DisablePlacements();

        private void DisablePlacements()
        {
            OnClearDynamicHighlight();
            mouseDistance = 0;
            PlacementCancel = true;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Placement Logic ◈◈◈◈◈◈                                                                              ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private int[] PlaceRegiments()
        {
            if (!IsVisibilityTrigger()) return Array.Empty<int>(); //Ordre => Garde la formation actuelle

            if (SelectedRegiments.Count is 1 && SelectedRegiments[0].CurrentFormation.NumUnitsAlive is 1)
            {
                return PlaceSingleUnitRegiment();
            }
            
            //Here! order is "random" fixe this! must keep formation OR reversed if direction is reversed
            float unitsToAddLength = mouseDistance - MinMaxSelectionWidth.x;
            NativeArray<int> newWidths = GetUpdatedFormationWidths(ref unitsToAddLength);
            NativeArray<float2> starts = GetStartsPosition(unitsToAddLength, newWidths);
            NativeList<JobHandle> jhs = GetInitialTokensPosition(starts, newWidths, out NativeArray<float2> initialTokensPositions);
            
            using NativeArray<RaycastHit> results = GetPositionAndRotationOnTerrain(ref initialTokensPositions, jhs);
            MoveHighlightsTokens(results);
            
            return newWidths.ToArray();
        }

        private int[] PlaceSingleUnitRegiment()
        {
            Vector3 origin3D = new Vector3(MouseStart.x, ORIGIN_HEIGHT, MouseStart.y);
            Ray ray = new Ray(origin3D, Vector3.down);
            if (!Raycast(ray, out RaycastHit hit, Infinity, TerrainLayer)) return Array.Empty<int>();
                
            int regimentId = SelectedRegiments[0].RegimentID;
            Vector3 hitPoint = MouseStart + hit.normal * 0.05f;
            Quaternion newRotation = LookRotationSafe(-DepthDirection, hit.normal);
            DynamicRegister.Records[regimentId][0].transform.SetPositionAndRotation(hitPoint, newRotation);
            return new int[]{1};
        }
        
        /// <summary>
        /// Initial Position from Start-Mouse to End-Mouse on the Map using only 2D coord
        /// </summary>
        /// <param name="starts">Starts calculated (Y component is incorrect at this point)</param>
        /// <param name="newWidths">Formations size foreach regiment allowed by the distance formed by Start-Mouse to End-Mouse</param>
        /// <param name="initialTokensPositions">Output: array of position calculated in 2D</param>
        /// <returns></returns>
        private NativeList<JobHandle> GetInitialTokensPosition(in NativeArray<float2> starts, in NativeArray<int> newWidths, out NativeArray<float2> initialTokensPositions)
        {
            // PHASE 1 : On récupère OU vont les tokens par rapport à la nouvelle formations
            half2 lineDirection2D = half2(LineDirection.xz);
            half2 depthDirection2D = half2(DepthDirection.xz);
            initialTokensPositions = new (TotalUnitsSelected, TempJob, UninitializedMemory);
            NativeList<JobHandle> jobHandles = new (SelectedRegiments.Count, Temp);
            
            int numUnitsRegimentsBefore = 0;
            for (int i = 0; i < SelectedRegiments.Count; i++)
            {
                FormationData regimentState = SelectedRegiments[i].CurrentFormation;
                JGetInitialTokensPositions job = new()
                {
                    NewWidth           = newWidths[i],
                    NumUnitsAlive      = regimentState.NumUnitsAlive,
                    DistanceUnitToUnit = half2(regimentState.DistanceUnitToUnit),
                    LineDirection      = lineDirection2D,
                    DepthDirection     = depthDirection2D,
                    Start              = starts[i],
                    TokensPositions    = initialTokensPositions.Slice(numUnitsRegimentsBefore, regimentState.NumUnitsAlive)
                };
                jobHandles.Add(job.ScheduleParallel(regimentState.NumUnitsAlive, JobWorkerCount-1, default));
                numUnitsRegimentsBefore += regimentState.NumUnitsAlive;
            }
            return jobHandles;
        }
        
        private NativeArray<RaycastHit> GetPositionAndRotationOnTerrain(ref NativeArray<float2> tokensPositions, NativeList<JobHandle> dependencies)
        {
            int totalUnitsSelected = TotalUnitsSelected;
            NativeArray<RaycastHit> results = new (totalUnitsSelected, TempJob, UninitializedMemory);
            using NativeArray<RaycastCommand> commands = new (totalUnitsSelected, TempJob, UninitializedMemory);
            // ---------------------------------------------------------------------------------------------------------
            // RAY CASTS
            NativeList<JobHandle> jobHandles = new (NumSelections, Temp);
            QueryParameters queryParams = new (TerrainLayer.value);
            
            int numUnitsRegimentBefore = 0;
            for (int i = 0; i < SelectedRegiments.Count; i++)
            {
                int numToken = SelectedRegiments[i].CurrentFormation.NumUnitsAlive;
                JRaycastsCommands raycastJob = new()
                {
                    OriginHeight = ORIGIN_HEIGHT,
                    RayDistance  = RAY_DISTANCE,
                    QueryParams  = queryParams,
                    Origins      = tokensPositions.Slice(numUnitsRegimentBefore, numToken),
                    Commands     = commands.Slice(numUnitsRegimentBefore, numToken)
                };
                jobHandles.Add(raycastJob.ScheduleParallel(numToken, JobWorkerCount-1, dependencies[i]));
                numUnitsRegimentBefore += numToken;
            }
            JobHandle combinedDependency = JobHandle.CombineDependencies(jobHandles.AsArray());
            JobHandle raycastCommandJh = RaycastCommand.ScheduleBatch(commands, results, 1, 1, combinedDependency);
            raycastCommandJh.Complete();
            tokensPositions.Dispose();
            return results;
        }
        
        private void MoveHighlightsTokens(NativeArray<RaycastHit> results)
        {
            float3 depthDirection = DepthDirection;
            int numUnitsRegimentBefore = 0;
            foreach (HighlightRegiment regiment in SelectedRegiments)
            {
                int regimentId = regiment.RegimentID;
                int numToken = regiment.CurrentFormation.NumUnitsAlive;
                NativeSlice<RaycastHit> raycastHits = results.Slice(numUnitsRegimentBefore, numToken);
                for (int unitIndex = 0; unitIndex < numToken; unitIndex++)
                {
                    RaycastHit currentHit = raycastHits[unitIndex];
                    Vector3 hitPoint = currentHit.point + currentHit.normal * 0.05f;
                    Quaternion newRotation = LookRotationSafe(-depthDirection, currentHit.normal);
                    DynamicRegister.Records[regimentId][unitIndex].transform.SetPositionAndRotation(hitPoint, newRotation);
                }
                numUnitsRegimentBefore += numToken;
            }
        }

        private NativeArray<int> GetUpdatedFormationWidths(ref float unitsToAddLength)
        {
            NativeArray<int> newWidths = MinWidthsArray;
            int attempts = 0;
            while (unitsToAddLength > 0 && attempts < newWidths.Length)
            {
                attempts = 0;
                for (int i = 0; i < newWidths.Length; i++)
                {
                    FormationData currentState = SelectedRegiments[i].CurrentFormation;
                    bool notEnoughSpace = unitsToAddLength < currentState.DistanceUnitToUnit.x;
                    bool isWidthAtMax   = newWidths[i] == currentState.MaxRow;
                    bool failAttempt    = notEnoughSpace || isWidthAtMax;
                    attempts           += failAttempt ? 1 : 0;
                    newWidths[i]       += !failAttempt ? 1 : 0;
                    unitsToAddLength   -= !failAttempt ? currentState.DistanceUnitToUnit.x : 0;
                }
            }
            return newWidths;
        }

        private NativeArray<float2> GetStartsPosition(float unitsToAddLength, NativeArray<int> newWidths)
        {
            float2 lineDirection = LineDirection.xz;
            bool isMaxDistanceReach = mouseDistance < MinMaxSelectionWidth.y;
            float leftOver = isMaxDistanceReach ? unitsToAddLength / (SelectedRegiments.Count - 1) : 0;
            
            NativeArray<float2> starts = new (SelectedRegiments.Count, Temp, UninitializedMemory);
            if (SelectedRegiments.Count is 0) return starts;
            
            starts[0] = ((float3)MouseStart).xz;
            for (int i = 1; i < SelectedRegiments.Count; i++)
            {
                float currUnitSpace  = SelectedRegiments[i].CurrentFormation.DistanceUnitToUnit.x;
                float prevUnitSpace  = SelectedRegiments[i - 1].CurrentFormation.DistanceUnitToUnit.x;
                float previousLength = (newWidths[i - 1] - 1) * prevUnitSpace; // -1 because we us space, not units
                previousLength      += csum(float2(prevUnitSpace, currUnitSpace) * 0.5f);//arrive at edge of last Unit + 1/2 newUnitSize
                previousLength      += DISTANCE_BETWEEN_REGIMENT + max(0, leftOver); // add regiment space
                
                starts[i] = starts[i - 1] + lineDirection * previousLength;
            }
            return starts;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇ Visibility Trigger ◇◇◇◇◇                                                                            │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        /// <summary>
        /// Is Cursor far enough to trigger event?
        /// </summary>
        /// <returns>condition met to trigger placement visibility</returns>
        private bool IsVisibilityTrigger()
        {
            if (PlacementsVisible) return true;
            if (mouseDistance < SelectedRegiments[0].CurrentFormation.DistanceUnitToUnit.x) return false;
            EnableAllDynamicSelected();
            return true;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇ Mouses Positions ◇◇◇◇◇                                                                              │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        private bool UpdateMouseStart(in Vector2 mouseInput)
        {
            Ray singleRay = PlayerCamera.ScreenPointToRay(mouseInput);
            bool isHit = Raycast(singleRay, out RaycastHit hit, Infinity, TerrainLayer);
            MouseStart = isHit ? hit.point : MouseStart;
            return isHit;
        }

        private bool UpdateMouseEnd(in Vector2 mouseInput)
        {
            Ray singleRay = PlayerCamera.ScreenPointToRay(mouseInput);
            bool isHit = Raycast(singleRay, out RaycastHit hit, Infinity, TerrainLayer);
            MouseEnd = isHit ? hit.point : MouseEnd;
            return isHit;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇ Toggle Methods ◇◇◇◇◇                                                                                │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void EnableAllDynamicSelected()
        {
            SelectedRegiments.ForEach(regiment => PlacementSystem.OnShow(regiment, PlacementSystem.DynamicRegisterIndex));
            PlacementsVisible = true;
        }
        
        private void OnClearDynamicHighlight()
        {
            PlacementSystem.HideAll(PlacementSystem.DynamicRegisterIndex);
            PlacementsVisible = false;
        }
        
        private void EnableAllStatic()
        {
            foreach ((int _, HighlightBehaviour[] tokens) in StaticRegister.Records)
            {
                if (tokens[0].IsShown()) continue;
                tokens.ForEach(token => token.Show());
            }
        }
        
        private void DisableAllStatic()
        {
            foreach ((int _, HighlightBehaviour[] tokens) in StaticRegister.Records)
            {
                if (tokens[0].IsHidden()) continue;
                tokens.ForEach(token => token.Hide());
            }
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ JOBS ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [BurstCompile]
        private struct JRaycastsCommands : IJobFor
        {
            [ReadOnly] public int OriginHeight;
            [ReadOnly] public int RayDistance;
            [ReadOnly] public QueryParameters QueryParams;
            
            [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeSlice<float2> Origins;
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public NativeSlice<RaycastCommand> Commands;

            public void Execute(int unitIndex)
            {
                float2 origin = Origins[unitIndex];
                Vector3 origin3D = new (origin.x, OriginHeight, origin.y);
                Commands[unitIndex] = new RaycastCommand(origin3D, Vector3.down, QueryParams, RayDistance);
            }
        }

        [BurstCompile]
        private struct JGetInitialTokensPositions : IJobFor
        {
            [ReadOnly] public int NewWidth;
            [ReadOnly] public int NumUnitsAlive;
            [ReadOnly] public half2 DistanceUnitToUnit;
            [ReadOnly] public half2 LineDirection;
            [ReadOnly] public half2 DepthDirection;
            [ReadOnly] public float2 Start;
            
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeSlice<float2> TokensPositions;

            public void Execute(int unitIndex)
            {
                int y = unitIndex / NewWidth;
                int x = unitIndex - (y * NewWidth);
                float2 yOffset = y * DistanceUnitToUnit.y * (float2)DepthDirection;
                float2 xOffset = x * DistanceUnitToUnit.x * (float2)LineDirection;
                float2 position = Start + GetStartOffset(y) + xOffset + yOffset;
                TokensPositions[unitIndex] = position;
            }

            private float2 GetStartOffset(int yUnit)
            {
                int maxDepth = (int)ceil((float)NumUnitsAlive / NewWidth);
                int numUnitLastLine = NumUnitsAlive - NewWidth * (maxDepth - 1);
                int diffLastLineWidth = NewWidth - numUnitLastLine;
                float offset = (diffLastLineWidth * 0.5f) * DistanceUnitToUnit.x;
                bool isLastLine = yUnit == maxDepth - 1;
                return select(0, (float2)LineDirection * offset, isLastLine);
            }
        }
    }
    
}
