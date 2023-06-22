using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;
using static PlayerControls;
using static UnityEngine.InputSystem.InputAction;

using ISelectableRegiment = KaizerWald.ISelectableRegiment;
using IUnit = KaizerWald.IUnit;

using Bounds = UnityEngine.Bounds;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace KaizerWald
{
    public sealed class RegimentSelectionController : CombinedController
    {
        public HighlightMediator Coordinator { get; private set; }

        private PreselectionActions preselectionControl;

        private readonly LayerMask SelectionLayer;
        private bool ClickDragPerformed;
        private Vector2 StartLMouse;
        private Vector2 EndLMouse;
        
        private readonly RaycastHit[] Hits = new RaycastHit[2];
        
        private InputAction CtrlAction;
        private bool IsCtrlPressed => CtrlAction.IsPressed();
        
        private HighlightSubSystem HoverSystem => CompositeSystem.SubSystem1;
        private HighlightSubSystem SelectionSystem => CompositeSystem.SubSystem2;
        
        public RegimentSelectionController(HighlightMediator coordinator, CompositeSystem compositeSystem, Camera camera, PlayerControls controls, LayerMask selectionLayer) : base(compositeSystem, camera)
        {
            Coordinator = coordinator;
            SelectionLayer = selectionLayer;
            preselectionControl = controls.Preselection;
            CtrlAction = preselectionControl.LockSelection;
            OnEnable();
        }
        
        public override void OnEnable()
        {
            if (!preselectionControl.enabled) preselectionControl.Enable();
            preselectionControl.MouseMove.performed             += OnMouseHover;
            preselectionControl.LeftMouseClickAndMove.started   += OnDragSelectionStart;
            preselectionControl.LeftMouseClickAndMove.performed += OnDragSelectionPerformed;
            preselectionControl.LeftMouseClickAndMove.canceled  += OnSelection;
        }

        public override void OnDisable()
        {
            preselectionControl.MouseMove.performed             -= OnMouseHover;
            preselectionControl.LeftMouseClickAndMove.started   -= OnDragSelectionStart;
            preselectionControl.LeftMouseClickAndMove.performed -= OnDragSelectionPerformed;
            preselectionControl.LeftMouseClickAndMove.canceled  -= OnSelection;
            preselectionControl.Disable();
        }
        
        // =============================================================================================================
        // -------- PRESELECTION ----------
        // =============================================================================================================
        
        //------------------------------------------------------------------------------------------------------------------
        //Single Unit Preselection
        private void OnMouseHover(CallbackContext context)
        {
            EndLMouse = context.ReadValue<Vector2>();
            if (ClickDragPerformed) return;
            CheckMouseHoverUnit();
        }
        
        //------------------------------------------------------------------------------------------------------------------
        //Multiple Unit Preselection
        private void OnDragSelectionStart(CallbackContext context)
        {
            StartLMouse = EndLMouse = context.ReadValue<Vector2>();
            ClickDragPerformed = false;
        }
        
        private void OnDragSelectionPerformed(CallbackContext context)
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
            ClearPreselection();
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
            ClearPreselection();
            AddPreselection(candidate);
        }

        private ISelectableRegiment GetPreselectionCandidate(in Ray singleRay, IUnit unit, int numHits)
        {
            ISelectableRegiment candidate = unit.SelectableRegimentAttach;
            if (numHits > 1 && !AreUnitsFromSameRegiment(candidate.RegimentID))
            {
                bool hit = Raycast(singleRay, out RaycastHit unitHit, INFINITY, SelectionLayer);
                candidate = !hit ? candidate : unitHit.transform.GetComponent<IUnit>().SelectableRegimentAttach;
            }
            return candidate;
        }

        private bool AreUnitsFromSameRegiment(int firstHitRegimentIndex)
        {
            int regimentId = Hits[1].transform.GetComponent<IUnit>().SelectableRegimentAttach.RegimentID;
            return firstHitRegimentIndex == regimentId;
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
            int numRegiment = Coordinator.Regiments.Count;
            Bounds selectionBounds = GetViewportBounds(StartLMouse, EndLMouse);

            for (int i = 0; i < numRegiment; i++)
            {
                if (Coordinator.Regiments[i] == null) continue;
                ISelectableRegiment selectableRegiment = Coordinator.Regiments[i];
                
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
        
        //--------------------------------------------------------------------------------------------------------------
        //Methods: Preselection
        //--------------------------------------------------------------------------------------------------------------
        private void AddPreselection(ISelectableRegiment selectableRegiment) => HoverSystem.OnShow(selectableRegiment);
        private void RemovePreselection(ISelectableRegiment selectableRegiment) => HoverSystem.OnHide(selectableRegiment);
        private void ClearPreselection() => HoverSystem.HideAll();
        
        // =============================================================================================================
        // -------- SELECTION ----------
        // =============================================================================================================
        
        public void OnSelection(CallbackContext context)
        {
            DeselectNotPreselected();
            SelectPreselection();
            //HighlightSystem.Register.OnNewSelection();//Experimental SelectionInfos
            
            void DeselectNotPreselected()
            {
                if (!IsCtrlPressed || HoverSystem.Register.ActiveHighlights.Count == 0) return;
                // we remove element from list, by iterating reverse we stay inbounds
                for (int i = HoverSystem.Register.ActiveHighlights.Count - 1; i > -1; i--)
                {
                    if (SelectionSystem.Register.ActiveHighlights[i].IsPreselected) continue;
                    SelectionSystem.OnHide(HoverSystem.Register.ActiveHighlights[i]);
                }
            }
            
            void SelectPreselection()
            {
                foreach (ISelectableRegiment selectable in HoverSystem.Register.ActiveHighlights)
                {
                    if (selectable.IsSelected) continue;
                    SelectionSystem.OnShow(selectable);
                }
            }
        }
    }
}
