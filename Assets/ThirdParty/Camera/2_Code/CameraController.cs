using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.Mathf;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.RTTCamera
{
    public class CameraController : MonoBehaviour, Controls.ICameraControlActions
    {
        [SerializeField, Min(0)] private float RotationSpeed = 1; 
        [SerializeField, Min(0)] private float BaseMoveSpeed = 1; 
        [SerializeField, Min(0)] private float ZoomSpeed = 1;

        [Tooltip("How far in degrees can you move the camera Down")]
        [SerializeField] private float MaxClamp = 70.0f;
        [Tooltip("How far in degrees can you move the camera Top")]
        [SerializeField] private float MinClamp = -30.0f;
        
        public bool DontDestroy;
        
        //Cache Data
        public Controls Controls { get; private set; }
        private Transform cameraTransform;

        //Inputs
        private bool isMoving;
        private bool isRotating;
        private bool isSprinting;
        private bool isZooming;
        private float zoom;

        private Vector2 mouseStartPosition, mouseEndPosition;
        private Vector2 moveAxis;
        
        //UPDATED MOVE SPEED
        private float SprintSpeed => BaseMoveSpeed * 2;
        private float MoveSpeed => isSprinting ? BaseMoveSpeed * SprintSpeed : BaseMoveSpeed;
        private void Awake()
        {
            if (Controls == null)
            {
                Controls = new Controls();
            }
            if (!Controls.CameraControl.enabled)
            {
                Controls.CameraControl.Enable();
                Controls.CameraControl.SetCallbacks(this);
            }
            cameraTransform = transform;
            if(DontDestroy) DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            if (!isRotating && !isMoving && !isZooming) return;
            
            // Rotation
            Quaternion newRotation = isRotating ? GetCameraRotation() : cameraTransform.rotation;

            // Position Left/Right/Front/Back
            Vector3 newPosition = isMoving ? GetCameraPosition(cameraTransform.position) : cameraTransform.position;

            // isZooming check not needed since we add 0 if zoom == 0
            newPosition += Vector3.up * (ZoomSpeed * zoom);// Position Up/Down
            
            cameraTransform.SetPositionAndRotation(newPosition, newRotation);
        }

        private Vector3 GetCameraPosition(in Vector3 cameraPosition)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            //real forward of the camera (aware of the rotation)
            Vector3 cameraForwardXZ = new (cameraForward.x, 0, cameraForward.z);
            
            Vector3 xDirection = Approximately(moveAxis.x,0) ? Vector3.zero : (moveAxis.x > 0 ? cameraRight : -cameraRight);
            Vector3 zDirection = Approximately(moveAxis.y,0) ? Vector3.zero : (moveAxis.y > 0 ? cameraForwardXZ : -cameraForwardXZ);

            float heightMultiplier = max(1f, cameraPosition.y); //plus la caméra est haute, plus elle est rapide
            float speedMultiplier  = heightMultiplier * MoveSpeed * Time.deltaTime;
            
            return cameraPosition + (xDirection + zDirection) * speedMultiplier;
        }

        private Quaternion GetCameraRotation()
        {
            //prevent calculation if middle mouse button hold without moving
            Quaternion rotation = cameraTransform.rotation;
            if (mouseEndPosition == mouseStartPosition) return rotation;

            Vector2 distanceXY = (mouseEndPosition - mouseStartPosition) * RotationSpeed;
            
            rotation = Utils.RotateFWorld(rotation, 0f, distanceXY.x * Time.deltaTime, 0f);//Rotation Horizontal
            rotation = Utils.RotateFSelf(rotation, -distanceXY.y * Time.deltaTime, 0f, 0f);//Rotation Vertical
            
            float clampedXAxis = Utils.ClampAngle(rotation.eulerAngles.x, MinClamp, MaxClamp);
            rotation.eulerAngles = new Vector3(clampedXAxis, rotation.eulerAngles.y, 0);
            
            mouseStartPosition = mouseEndPosition; //reset start position
            return rotation;
        }
        
        //==============================================================================================================
        // INPUTS EVENTS CALLBACK
        
        public void OnMouvement(InputAction.CallbackContext context)
        {
            isMoving = !context.canceled;
            moveAxis = isMoving ? context.ReadValue<Vector2>() : Vector2.zero;
        }

        public void OnRotation(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    mouseStartPosition = context.ReadValue<Vector2>();
                    isRotating = true;
                    break;
                case InputActionPhase.Performed:
                    mouseEndPosition = context.ReadValue<Vector2>();
                    break;
                case InputActionPhase.Canceled:
                    isRotating = false;
                    break;
                default:
                    return;
            }
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            isZooming = !context.canceled;
            zoom = isZooming ? context.ReadValue<float>() : 0;
        }

        public void OnFaster(InputAction.CallbackContext context)
        {
            isSprinting = !context.canceled;
        }
        //==============================================================================================================
    }
}
