using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
//using KWUtils;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;
/*
using static KWUtils.UGuiUtils;


using static KWUtils.InputSystemExtension;
*/
using static PlayerControls;
using static UnityEngine.InputSystem.InputAction;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace KaizerWald
{
    public sealed class PreselectionController : HighlightController
    {
        private PreselectionActions preselectionControl;
        
        private readonly LayerMask SelectionLayer;
        
        private bool ClickDragPerformed;
        private Vector2 StartLMouse;
        private Vector2 EndLMouse;
        
        //Raycast
        private readonly RaycastHit[] Hits = new RaycastHit[2];

        public PreselectionController(HighlightSystem system, LayerMask selectionLayer, PlayerControls controls): base(system, Camera.main)
        {
            SelectionLayer = selectionLayer;
            preselectionControl = controls.Preselection;
            OnEnable();
        }
        
        public override void OnEnable()
        {
            if (!preselectionControl.enabled) preselectionControl.Enable();
            preselectionControl.MouseMove.performed             += OnMouseMove;
            preselectionControl.LeftMouseClickAndMove.started   += OnLeftMouseClickAndMoveStart;
            preselectionControl.LeftMouseClickAndMove.performed += OnLeftMouseClickAndMovePerformed;
            preselectionControl.LeftMouseClickAndMove.canceled  += TestFireAndForget;
        }

        public override void OnDisable()
        {
            preselectionControl.MouseMove.performed             -= OnMouseMove;
            preselectionControl.LeftMouseClickAndMove.started   -= OnLeftMouseClickAndMoveStart;
            preselectionControl.LeftMouseClickAndMove.performed -= OnLeftMouseClickAndMovePerformed;
            preselectionControl.LeftMouseClickAndMove.canceled  -= TestFireAndForget;
            preselectionControl.Disable();
        }
        
        // =============================================================================================================
        // -------- On Update ----------
        // =============================================================================================================
        
        
        // =============================================================================================================
        // -------- Event Dispatcher ----------
        // =============================================================================================================

        private bool Test()
        {
            return HighlightSystem.Register.ActiveHighlights.All(test => test.IsSelected);
        }

        public void TestFireAndForget(CallbackContext context)
        {
            OnLeftMouseCancel().Forget();
        }
        
        public async UniTaskVoid OnLeftMouseCancel()
        {
            if (HighlightSystem.Register.ActiveHighlights.Count == 0) return;
            Debug.Log($"Test ok: {HighlightSystem.Register.ActiveHighlights.Count}");
            //await UniTask.NextFrame();
            await UniTask.WaitUntil(Test);
            if (!ClickDragPerformed) return;
            CheckMouseHoverUnit();
            ClickDragPerformed = false;
            Debug.Log("Test ok");
        }
        /*
        //Event Dispatched by SystemController
        public void OnLeftMouseCancel(CallbackContext context)
        {
            HighlightSystem.Notify();
            if (!ClickDragPerformed) return;
            CheckMouseHoverUnit();
            ClickDragPerformed = false;
        }
        */
        // =============================================================================================================
        // -------- Event Based Controls ----------
        // =============================================================================================================
        
        //------------------------------------------------------------------------------------------------------------------
        //Single Unit Preselection
        private void OnMouseMove(CallbackContext context)
        {
            EndLMouse = context.ReadValue<Vector2>();
            if (ClickDragPerformed) return;
            CheckMouseHoverUnit();
        }
        
        //------------------------------------------------------------------------------------------------------------------
        //Multiple Unit Preselection
        private void OnLeftMouseClickAndMoveStart(CallbackContext context)
        {
            StartLMouse = EndLMouse = context.ReadValue<Vector2>();
            ClickDragPerformed = false;
        }
        
        private void OnLeftMouseClickAndMovePerformed(CallbackContext context)
        {
            ClickDragPerformed = IsDragSelection();
            if (!ClickDragPerformed) return;
            PreselectionMethodChoice();
        }
        
        private bool IsDragSelection() => lengthsq(EndLMouse - StartLMouse) >= 128;
        
        //------------------------------------------------------------------------------------------------------------------
        //Mouse Hover Check
        
        //Guard Clause : Num Hits
        private bool NoHits(int numHits)
        {
            if (numHits != 0) return false;
            ClearPreselections();
            return true;
        }
        
        private void CheckMouseHoverUnit()
        {
            const float sphereRadius = 0.5f;
            Ray singleRay = PlayerCamera.ScreenPointToRay(EndLMouse);
            int numHits = SphereCastNonAlloc(singleRay, sphereRadius, Hits,INFINITY,SelectionLayer);

            if (NoHits(numHits)) return;
            MouseHoverSingleEntity(singleRay, numHits);
            
            if(Hits.Length == 0) return;
            Array.Clear(Hits, 0, Hits.Length);
        }
        
        //------------------------------------------------------------------------------------------------------------------
        //Mouse Hover : No Drag
        private void MouseHoverSingleEntity(in Ray singleRay, int numHits)
        {
            if (Hits[0].transform == null) return;
            if (!Hits[0].transform.TryGetComponent(out IUnit unit))
            {
                Debug.Log($"Dont have Component: Unit {Hits[0].transform.name}");
                return;
            }
            ISelectableRegiment candidate = GetPreselectionCandidate(singleRay, unit, numHits);
            if (candidate.IsPreselected) return;
            ClearPreselections();
            AddPreselection(candidate);
            
            //Notify(candidate.RegimentID) //OnSinglePreselectionEvent
        }

        private ISelectableRegiment GetPreselectionCandidate(in Ray singleRay, IUnit unit, int numHits)
        {
            ISelectableRegiment candidate = unit.SelectableRegimentAttach;
            if (numHits > 1 && !AreUnitsFromSameRegiment(candidate.RegimentID/*, numHits*/))
            {
                bool hit = Raycast(singleRay, out RaycastHit unitHit, INFINITY, SelectionLayer);
                candidate = !hit ? candidate : unitHit.transform.GetComponent<IUnit>().SelectableRegimentAttach;
            }
            return candidate;
        }

        private bool AreUnitsFromSameRegiment(int firstHitRegimentIndex/*, int numUnitsHits*/)
        {
            int regimentId = Hits[1].transform.GetComponent<IUnit>().SelectableRegimentAttach.RegimentID;
            return firstHitRegimentIndex == regimentId;
            /*
            if (firstHitRegimentIndex != regimentId) return false;
            for (int i = 1; i < numUnitsHits; i++) // firstHitRegimentIndex is index 0, so we start at 1
            {
                int regimentId = Hits[i].transform.GetComponent<IUnit>().SelectableRegimentAttach.RegimentID;
                if (firstHitRegimentIndex != regimentId) return false;
            }
            return true;
            */
        }

        //------------------------------------------------------------------------------------------------------------------
        //Preselection
        
        /// <summary>
        /// ADD Job system Later
        /// </summary>
        private void PreselectionMethodChoice()
        {
            PreselectRegiments();
        }
        
        private void PreselectRegiments()
        {
            int numRegiment = HighlightSystem.Coordinator.Regiments.Count;
            Bounds selectionBounds = GetViewportBounds(StartLMouse, EndLMouse);

            for (int i = 0; i < numRegiment; i++)
            {
                if (HighlightSystem.Coordinator.Regiments[i] == null) continue;
                ISelectableRegiment selectableRegiment = HighlightSystem.Coordinator.Regiments[i];
                
                bool isInSelectionRectangle = CheckUnitsInRectangleBounds(selectableRegiment);

                if(!selectableRegiment.IsPreselected && isInSelectionRectangle) 
                    AddPreselection(selectableRegiment);
                else if(selectableRegiment.IsPreselected && !isInSelectionRectangle) 
                    RemovePreselection(selectableRegiment);
            }
            
            bool CheckUnitsInRectangleBounds(ISelectableRegiment regiment)
            {
                foreach (Transform unitTransform in regiment.UnitsTransform)
                {
                    if (unitTransform == null) continue;
                    Vector3 unitPositionInRect = PlayerCamera.WorldToViewportPoint(unitTransform.position);
                    if (!selectionBounds.Contains(unitPositionInRect)) continue;
                    return true;
                }
                return false;
            }
        }

        private Bounds GetViewportBounds(in Vector3 startPoint, in Vector3 endPoint)
        {
            Vector3 start = PlayerCamera.ScreenToViewportPoint(startPoint);
            Vector3 end = PlayerCamera.ScreenToViewportPoint(endPoint);
            Vector3 min = Vector3.Min(start, end);
            Vector3 max = Vector3.Max(start, end);
            min.z = PlayerCamera.nearClipPlane;
            max.z = PlayerCamera.farClipPlane;
            Bounds bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }
        
        //==================================================================================================================
        //Methods: Preselection
        //==================================================================================================================
        private void AddPreselection(ISelectableRegiment selectableRegiment) => HighlightSystem.OnShow(selectableRegiment);
        private void RemovePreselection(ISelectableRegiment selectableRegiment) => HighlightSystem.OnHide(selectableRegiment);
        private void ClearPreselections() => HighlightSystem.HideAll();
    }
}