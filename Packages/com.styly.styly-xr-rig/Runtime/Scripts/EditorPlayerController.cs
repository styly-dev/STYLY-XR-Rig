using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

namespace Styly.XRRig
{
    /// <summary>
    /// Controller for operating local avatar in the editor
    /// </summary>
#if UNITY_EDITOR
    public class EditorPlayerController : MonoBehaviour
    {
        [Header("Target")][SerializeField] private Transform target;

        [Header("Movement Settings (WASD + EQ)")]
        [SerializeField]
        private float moveSpeed = 5f;

        [Header("Look Settings")]
        [SerializeField]
        private float lookSensitivity = 8f;

        [Header("Snap Turn (Left/Right Arrow Keys)")]
        [SerializeField]
        private float snapTurnDegrees = 45f;

        private float snapTurnDebounce = 0.25f;

        private Transform controlTarget;
        private Transform cameraTransform;
        private Transform cameraOffsetTransform;
        private float rotationX = 0f;
        private float rotationY = 0f;
        private float lastSnapTime = -999f;

        void Start()
        {
            // Set control target (use self if target is null)
            controlTarget = target != null ? target : transform;

            // Get Main Camera and Camera Offset transforms
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                // Camera Offset is the parent of Main Camera
                if (cameraTransform.parent != null && cameraTransform.parent != controlTarget)
                {
                    cameraOffsetTransform = cameraTransform.parent;
                }
            }

            // Preserve current rotation based on camera's world rotation
            if (cameraTransform != null)
            {
                rotationY = cameraTransform.eulerAngles.y;
                rotationX = cameraTransform.eulerAngles.x;
            }
            else
            {
                rotationY = controlTarget.eulerAngles.y;
                rotationX = controlTarget.eulerAngles.x;
            }
        }

        void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleSnapTurn();
        }

        /// <summary>
        /// Movement handling with WASD (XZ plane) and EQ (Y axis)
        /// </summary>
        private void HandleMovement()
        {
            Vector3 moveDirection = Vector3.zero;
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null) return;

            // Use camera's forward/right to account for Camera Offset rotation
            Transform directionSource = cameraTransform != null ? cameraTransform : controlTarget;

            // XZ plane movement (WASD)
            if (keyboard.wKey.isPressed)
                moveDirection += directionSource.forward;
            if (keyboard.sKey.isPressed)
                moveDirection -= directionSource.forward;
            if (keyboard.aKey.isPressed)
                moveDirection -= directionSource.right;
            if (keyboard.dKey.isPressed)
                moveDirection += directionSource.right;

            // Y axis movement (E/Q)
            if (keyboard.eKey.isPressed)
                moveDirection += Vector3.up;
            if (keyboard.qKey.isPressed)
                moveDirection -= Vector3.up;

            // Apply movement
            if (moveDirection.magnitude > 0)
            {
                moveDirection.Normalize();
                controlTarget.position += moveDirection * moveSpeed * Time.deltaTime;
            }
        }

        /// <summary>
        /// View rotation handling with right mouse button drag
        /// </summary>
        private void HandleRotation()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            // Rotate only while right-clicking
            if (mouse.rightButton.isPressed)
            {
                Vector2 mouseDelta = mouse.delta.ReadValue() * lookSensitivity * 0.1f;

                rotationY += mouseDelta.x;
                rotationX -= mouseDelta.y;

                // Vertical angle limit
                rotationX = Mathf.Clamp(rotationX, -90f, 90f);

                // Desired camera world rotation
                Quaternion desiredRotation = Quaternion.Euler(rotationX, rotationY, 0);

                // Rotate around camera position so the view doesn't orbit when Camera Offset has a position
                RotateControlTargetForCamera(desiredRotation);
            }
        }

        /// <summary>
        /// Snap turn handling with left/right arrow keys
        /// </summary>
        private void HandleSnapTurn()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (controlTarget == null) return;

            bool rightPressed = keyboard.rightArrowKey.wasPressedThisFrame;
            bool leftPressed = keyboard.leftArrowKey.wasPressedThisFrame;

            if (!rightPressed && !leftPressed) return;

            float now = Time.time;
            if (now - lastSnapTime < snapTurnDebounce) return;

            float direction = rightPressed ? 1f : -1f;
            if (rightPressed && leftPressed)
            {
                direction = 0f;
            }

            if (Mathf.Approximately(direction, 0f)) return;

            rotationY += snapTurnDegrees * direction;
            Quaternion desiredRotation = Quaternion.Euler(rotationX, rotationY, 0);
            RotateControlTargetForCamera(desiredRotation);
            lastSnapTime = now;
        }

        /// <summary>
        /// Set controlTarget rotation so the camera achieves the desired world rotation,
        /// adjusting position to rotate around the camera (not the rig root).
        /// </summary>
        private void RotateControlTargetForCamera(Quaternion desiredCameraRotation)
        {
            if (cameraTransform == null)
            {
                controlTarget.rotation = desiredCameraRotation;
                return;
            }

            // Remember camera world position before rotation
            Vector3 cameraWorldPos = cameraTransform.position;

            // Compute controlTarget rotation that achieves desired camera rotation
            if (cameraOffsetTransform != null)
            {
                controlTarget.rotation = desiredCameraRotation * Quaternion.Inverse(cameraOffsetTransform.localRotation);
            }
            else
            {
                controlTarget.rotation = desiredCameraRotation;
            }

            // After rotation, camera has moved — shift controlTarget so camera returns to its original position
            Vector3 cameraNewPos = cameraTransform.position;
            controlTarget.position += cameraWorldPos - cameraNewPos;
        }
    }
#else
    public class EditorPlayerController : MonoBehaviour
    {
        // Do nothing outside the editor
    }
#endif
}
