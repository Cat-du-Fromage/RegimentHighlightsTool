using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using static Unity.Mathematics.math;
using static KaizerWaldCode.RTTCamera.UGuiUtils;

namespace KaizerWaldCode.RTTCamera
{
    [RequireComponent(typeof(CameraController))]
    public class SelectionRectangle : MonoBehaviour, Controls.ISelectionRectangleActions
    {
        private CameraController cameraController;
        
        [field:SerializeField] public bool ClickDragPerformed{ get; private set; }
        [field:SerializeField] public Vector2 StartLMouse{ get; private set; }
        [field:SerializeField] public Vector2 EndLMouse{ get; private set; }

        private void Awake()
        {
            cameraController = GetComponent<CameraController>();
        }

        private void Start()
        {
            cameraController.Controls.SelectionRectangle.Enable();
            cameraController.Controls.SelectionRectangle.SetCallbacks(this);
        }

        private void OnDestroy()
        {
            cameraController.Controls.SelectionRectangle.Disable();
        }

        //==================================================================================================================
        //Rectangle OnScreen
        //==================================================================================================================
        private void OnGUI()
        {
            if (!ClickDragPerformed) return;
            // Create a rect from both mouse positions
            Rect rect = GetScreenRect(StartLMouse, EndLMouse);
            DrawScreenRect(rect);
            DrawScreenRectBorder(rect, 1);
        }
        
        private bool IsDragSelection() => Vector2.SqrMagnitude(EndLMouse - StartLMouse) >= 128;
        public void OnLeftMouseClickAndMove(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                StartLMouse = EndLMouse = context.ReadValue<Vector2>();
                ClickDragPerformed = false;
            }
            else if(context.performed)
            {
                EndLMouse = context.ReadValue<Vector2>();
                ClickDragPerformed = IsDragSelection();
            }
            else
            {
                ClickDragPerformed = false;
            }
        }
    }
}
