using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;
using static PlayerControls;
using static UnityEngine.InputSystem.InputAction;

using Bounds = UnityEngine.Bounds;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace KaizerWald
{
    public sealed class RegimentSelectionController : HighlightController
    {
        private SelectionActions selectionControl;

        private readonly LayerMask SelectionLayer;
        private bool ClickDragPerformed;
        private Vector2 StartLMouse, EndLMouse;
        private readonly RaycastHit[] Hits = new RaycastHit[2];

        public SelectionSystem SelectionSystem { get; private set; }
        private List<SelectableRegiment> Regiments => SelectionSystem.Regiments;
        private HighlightRegister PreselectionRegister => SelectionSystem.PreselectionRegister;
        private HighlightRegister SelectionRegister => SelectionSystem.SelectionRegister;
        
        private bool IsCtrlPressed => selectionControl.LockSelection.IsPressed();
        
        public RegimentSelectionController(HighlightSystem system, PlayerControls controls, LayerMask unitLayer) : base(system)
        {
            SelectionSystem = (SelectionSystem)system;
            SelectionLayer = unitLayer;
            selectionControl = controls.Selection;
            OnEnable();
        }
        
        public override void OnEnable()
        {
            if (!selectionControl.enabled) selectionControl.Enable();
            selectionControl.MouseMove.performed             += OnMouseHover;
            selectionControl.LeftMouseClickAndMove.started   += OnDragSelectionStart;
            selectionControl.LeftMouseClickAndMove.performed += OnDragSelectionPerformed;
            selectionControl.LeftMouseClickAndMove.canceled  += OnSelection;
        }

        public override void OnDisable()
        {
            selectionControl.MouseMove.performed             -= OnMouseHover;
            selectionControl.LeftMouseClickAndMove.started   -= OnDragSelectionStart;
            selectionControl.LeftMouseClickAndMove.performed -= OnDragSelectionPerformed;
            selectionControl.LeftMouseClickAndMove.canceled  -= OnSelection;
            selectionControl.Disable();
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
            SelectableRegiment candidate = GetPreselectionCandidate(singleRay, unit, numHits);
            if (candidate.IsPreselected) return;
            ClearPreselection();
            AddPreselection(candidate);
        }

        private SelectableRegiment GetPreselectionCandidate(in Ray singleRay, IUnit unit, int numHits)
        {
            SelectableRegiment candidate = unit.SelectableRegimentAttach;
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
            Bounds selectionBounds = GetViewportBounds(StartLMouse, EndLMouse);
            foreach (SelectableRegiment regiment in Regiments)
            {
                if (regiment == null) continue;
                bool isInSelectionRectangle = CheckUnitsInRectangleBounds(regiment);
                
                if(!regiment.IsPreselected && isInSelectionRectangle) 
                    AddPreselection(regiment);
                else if(regiment.IsPreselected && !isInSelectionRectangle) 
                    RemovePreselection(regiment);
            }
            
            bool CheckUnitsInRectangleBounds(SelectableRegiment regiment)
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
        private void AddPreselection(SelectableRegiment selectableRegiment)
        {
            SelectionSystem.OnShow(selectableRegiment, (int)ESelection.Preselection);
        }

        private void RemovePreselection(SelectableRegiment selectableRegiment)
        {
            SelectionSystem.OnHide(selectableRegiment, (int)ESelection.Preselection);
        }

        private void ClearPreselection()
        {
            SelectionSystem.HideAll((int)ESelection.Preselection);
        }

        // =============================================================================================================
        // -------- SELECTION ----------
        // =============================================================================================================
        
        private void OnSelection(CallbackContext context)
        {
            DeselectNotPreselected();
            SelectPreselection();
            //HighlightSystem.Register.OnNewSelection();//Experimental SelectionInfos
            
            if (!ClickDragPerformed) return;
            CheckMouseHoverUnit();
            ClickDragPerformed = false;
            
            void DeselectNotPreselected()
            {
                if (IsCtrlPressed) return;
                // we remove element from list, by iterating reverse we stay inbounds
                for (int i = SelectionRegister.ActiveHighlights.Count - 1; i > -1; i--)
                {
                    SelectableRegiment regiment = SelectionRegister.ActiveHighlights[i];
                    if (regiment.IsPreselected) continue;
                    SelectionSystem.OnHide(regiment, (int)ESelection.Selection);
                }
            }
            
            void SelectPreselection()
            {
                foreach (SelectableRegiment selectable in PreselectionRegister.ActiveHighlights)
                {
                    if (selectable.IsSelected) continue;
                    SelectionSystem.OnShow(selectable, (int)ESelection.Selection);
                }
            }
        }
    }
}
