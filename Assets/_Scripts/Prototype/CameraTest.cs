using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWald
{
    public class CameraTest : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 1;
        private Vector2 startMouse, endMouse;

        private Mouse mouse;

        private void Awake()
        {
            mouse = Mouse.current;
        }

        private void Update()
        {
            RotationCamera();
        }

        private void RotationCamera()
        {
            if (mouse.middleButton.wasPressedThisFrame)
            {
                startMouse = mouse.position.value;
            }
            if (mouse.middleButton.isPressed)
            {
                endMouse = mouse.position.value;
                Vector2 delta = (endMouse - startMouse) * (rotationSpeed * Time.deltaTime);
                transform.Rotate(0,delta.x,0, Space.World);
                transform.Rotate(Mathf.Abs(delta.y),0,0, Space.Self);
                startMouse = endMouse;
            };
        }
    }
}
