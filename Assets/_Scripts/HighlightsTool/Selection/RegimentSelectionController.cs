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
        private const float SPHERE_RADIUS = 0.5f;
        private const float RAYCAST_DISTANCE = ushort.MaxValue;
        
        private readonly LayerMask SelectionLayer;
        private readonly RaycastHit[] Hits = new RaycastHit[2];
        
        private SelectionActions selectionControl;
        private bool ClickDragPerformed;
        private Vector2 StartLMouse, EndLMouse;
        
        public SelectionSystem SelectionSystem { get; private set; }
        private HighlightRegister PreselectionRegister => SelectionSystem.PreselectionRegister;
        private HighlightRegister SelectionRegister => SelectionSystem.SelectionRegister;
        
        private bool IsCtrlPressed => selectionControl.LockSelection.IsPressed();
        
        public RegimentSelectionController(HighlightSystem system, PlayerControls controls, LayerMask unitLayer)
        {
            SelectionSystem = (SelectionSystem)system;
            SelectionLayer = unitLayer;
            selectionControl = controls.Selection;
            //OnEnable();
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

        private Vector3 LastPositionOnTerrain;
        public override void OnUpdate()
        {
            //We also need to check when Units Move not only when camera move
            if (ClickDragPerformed) return;
            CheckMouseHoverUnit();
        }
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ PRESELECTION ◇◇◇◇◇                                                                                   ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ EVENT FUNCTIONS ◇◇◇◇◇                                                                                ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◆◆◆◆ Single Unit Preselection ◆◆◆◆                                                                        │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void OnMouseHover(CallbackContext context)
        {
            EndLMouse = context.ReadValue<Vector2>();
            //if (ClickDragPerformed) return;
            //CheckMouseHoverUnit();
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◆◆◆◆ Multiple Unit Preselection ◆◆◆◆                                                                      │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void OnDragSelectionStart(CallbackContext context)
        {
            StartLMouse = EndLMouse = context.ReadValue<Vector2>();
            ClickDragPerformed = false;
        }
        
        private void OnDragSelectionPerformed(CallbackContext context)
        {
            ClickDragPerformed = IsDragSelection();
            if (!ClickDragPerformed) return;
            GroupPreselectionRegiments();
        }
        
        private bool IsDragSelection() => Vector2.SqrMagnitude(EndLMouse - StartLMouse) >= 128;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◆◆◆◆ Mouse Hover Check ◆◆◆◆                                                                               │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        private bool NoHits(int numHits)
        {
            if (numHits != 0) return false;
            ClearPreselection();
            return true;
        }
        
        private void CheckMouseHoverUnit()
        {
            Ray singleRay = PlayerCamera.ScreenPointToRay(EndLMouse);
            int numHits = SphereCastNonAlloc(singleRay, SPHERE_RADIUS, Hits,RAYCAST_DISTANCE, SelectionLayer.value);
            if (NoHits(numHits)) return;
            MouseHoverSingleEntity(singleRay, numHits);
            
            if(Hits.Length is 0) return;
            Array.Clear(Hits, 0, Hits.Length);
        }
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ PRESELECTION METHODS ◇◇◇◇◇                                                                           ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◆◆◆◆ Single Preselection ◆◆◆◆                                                                             │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void MouseHoverSingleEntity(in Ray singleRay, int numHits)
        {
            //if (Hits[0].transform == null) return;
            if (!Hits[0].transform.TryGetComponent(out Unit unit))
            {
                Debug.LogError($"Dont have Component: Unit {Hits[0].transform.name}");
                return;
            }
            Regiment candidate = GetPreselectionCandidate(singleRay, unit, numHits);
            if (candidate.IsPreselected) return;
            ClearPreselection();
            AddPreselection(candidate);
        }

        private Regiment GetPreselectionCandidate(in Ray singleRay, Unit unit, int numHits)
        {
            Regiment candidate = unit.RegimentAttach;
            if (numHits > 1 && !AreUnitsFromSameRegiment(candidate.RegimentID))
            {
                bool hit = Raycast(singleRay, out RaycastHit unitHit, INFINITY, SelectionLayer.value);
                candidate = !hit ? candidate : unitHit.transform.GetComponent<Unit>().RegimentAttach;
            }
            return candidate;
        }

        private bool AreUnitsFromSameRegiment(int firstHitRegimentIndex)
        {
            int regimentId = Hits[1].transform.GetComponent<Unit>().RegimentAttach.RegimentID;
            return firstHitRegimentIndex == regimentId;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◆◆◆◆ Group Preselection ◆◆◆◆                                                                              │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void GroupPreselectionRegiments()
        {
            Bounds selectionBounds = GetViewportBounds(StartLMouse, EndLMouse);
            foreach (Regiment regiment in SelectionSystem.Regiments)
            {
                if (regiment == null) continue;
                bool isInSelectionRectangle = CheckUnitsInRectangleBounds(regiment);
                
                if(!regiment.IsPreselected && isInSelectionRectangle) 
                    AddPreselection(regiment);
                else if(regiment.IsPreselected && !isInSelectionRectangle) 
                    RemovePreselection(regiment);
            }
            
            bool CheckUnitsInRectangleBounds(Regiment regiment)
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
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Method Preselection ◇◇◇◇◇                                                                            ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void AddPreselection(Regiment selectableRegiment)
        {
            SelectionSystem.OnShow(selectableRegiment, SelectionSystem.PreselectionRegisterIndex);
        }

        private void RemovePreselection(Regiment selectableRegiment)
        {
            SelectionSystem.OnHide(selectableRegiment, SelectionSystem.PreselectionRegisterIndex);
        }

        private void ClearPreselection()
        {
            SelectionSystem.HideAll(SelectionSystem.PreselectionRegisterIndex);
        }
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ SELECTION ◇◇◇◇◇                                                                                      ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void OnSelection(CallbackContext context)
        {
            DeselectNotPreselected();
            SelectPreselection();
            if (!ClickDragPerformed) return;
            CheckMouseHoverUnit();
            ClickDragPerformed = false;
        }
        
        private void SelectPreselection()
        {
            foreach (Regiment selectable in PreselectionRegister.ActiveHighlights)
            {
                if (selectable.IsSelected) continue;
                SelectionSystem.OnShow(selectable, SelectionSystem.SelectionRegisterIndex);
            }
        }
        
        private void DeselectNotPreselected()
        {
            if (IsCtrlPressed) return;
            // we remove element from list, by iterating reverse we stay inbounds
            for (int i = SelectionRegister.ActiveHighlights.Count - 1; i > -1; i--)
            {
                Regiment regiment = SelectionRegister.ActiveHighlights[i];
                if (regiment.IsPreselected) continue;
                SelectionSystem.OnHide(regiment, SelectionSystem.SelectionRegisterIndex);
            }
        }
    }
}
