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
            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                startMouse = Mouse.current.position.value;
            }
            if (Mouse.current.middleButton.isPressed)
            {
                endMouse = Mouse.current.position.value;
                Vector2 delta = (endMouse - startMouse) * (rotationSpeed * Time.deltaTime);
                transform.Rotate(0,delta.x,0, Space.World);
                transform.Rotate(-delta.y,0,0, Space.Self);
                startMouse = endMouse;
            }
        }
    }
}
