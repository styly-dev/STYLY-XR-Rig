using UnityEngine;
using UnityEngine.InputSystem;

namespace Styly.XRRig
{
    /// <summary>
    /// Controller for operating local avatar in the editor
    /// </summary>
#if UNITY_EDITOR
    public class EditorPlayerController : MonoBehaviour
    {
        [Header("Target")] [SerializeField] private Transform target;

        [Header("Movement Settings")] [SerializeField]
        private float moveSpeed = 5f;

        [Header("Look Settings")] [SerializeField]
        private float lookSensitivity = 8f;

        private Transform controlTarget;
        private float rotationX = 0f;
        private float rotationY = 0f;

        void Start()
        {
            // Set control target (use self if target is null)
            controlTarget = target != null ? target : transform;

            // Preserve current rotation
            rotationY = controlTarget.eulerAngles.y;
            rotationX = controlTarget.eulerAngles.x;
        }

        void Update()
        {
            HandleMovement();
            HandleRotation();
        }

        /// <summary>
        /// Movement handling with WASD (XZ plane) and EQ (Y axis)
        /// </summary>
        private void HandleMovement()
        {
            Vector3 moveDirection = Vector3.zero;
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null) return;

            // XZ plane movement (WASD)
            if (keyboard.wKey.isPressed)
                moveDirection += controlTarget.forward;
            if (keyboard.sKey.isPressed)
                moveDirection -= controlTarget.forward;
            if (keyboard.aKey.isPressed)
                moveDirection -= controlTarget.right;
            if (keyboard.dKey.isPressed)
                moveDirection += controlTarget.right;

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

                controlTarget.rotation = Quaternion.Euler(rotationX, rotationY, 0);
            }
        }
    }
#else
    public class EditorPlayerController : MonoBehaviour
    {
        // Do nothing outside the editor
    }
#endif
}