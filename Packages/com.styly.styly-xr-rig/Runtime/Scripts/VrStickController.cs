using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Styly.XRRig
{
    public class VrStickController : MonoBehaviour
    {
        [Header("Input Actions (assign in Inspector)")]
        [Tooltip("Assign \"XRI Left Locomotion/Move\" from Starter Assets/XRI Default Input Actions")]
        public InputActionProperty moveAction;

        [Tooltip("Assign \"XRI Right Locomotion/Snap Turn\" from Starter Assets/XRI Default Input Actions")]
        public InputActionProperty snapTurnAction;

        // Head (Main Camera) will be assigned
        private Transform headTransform;

        // STYLY XR Rig root will be assigned
        private Transform moveTarget;

        [Tooltip("Move speed (m/s)")]
        public float moveSpeed = 2.0f;

        [Tooltip("Ignore Y component and move along the horizontal plane")]
        public bool flattenToGroundPlane = true;

        [Header("Snap Turn")]
        [Tooltip("Snap turn angle per step (degrees)")]
        public float snapTurnDegrees = 45f;

        [Tooltip("Snap turn input threshold (right: +threshold / left: -threshold)")]
        [Range(0.1f, 0.95f)]
        public float snapTurnDeadzone = 0.5f;

        [Tooltip("Minimum interval between consecutive snap turns in the same direction (seconds)")]
        public float snapTurnDebounce = 0.25f;

        // Internal state
        float _prevSnapValue = 0f;
        float _lastSnapTime = -999f;

        void Awake()
        {
            // Use Main Camera if not explicitly specified
            if (headTransform == null && Camera.main != null)
                headTransform = Camera.main.transform;
        }

        void Start()
        {
            if (Camera.main != null) headTransform = Camera.main.transform;
            moveTarget = FindFirstObjectByType<Styly.XRRig.StylyXrRig>().transform;
        }

        void OnEnable()
        {
            // Enable InputActions (generally safe even with Input Action Manager)
            if (moveAction.action != null) moveAction.action.Enable();
            if (snapTurnAction.action != null) snapTurnAction.action.Enable();
        }

        void OnDisable()
        {
            if (moveAction.action != null) moveAction.action.Disable();
            if (snapTurnAction.action != null) snapTurnAction.action.Disable();
        }

        void Update()
        {
            HandleMove();
            HandleSnapTurn();
        }

        void HandleMove()
        {
            if (moveAction.action == null) return;

            if (moveTarget == null)
            {
                Debug.LogError("VrStickController: moveTarget is not set. Please assign the Transform to move in the Inspector.");
                return;
            }

            Vector2 move = Vector2.zero;
            try { move = moveAction.action.ReadValue<Vector2>(); }
            catch { /* Fallback for unassigned or type mismatch */ }

            if (move.sqrMagnitude < 0.0001f) return;

            // Forward and right vectors based on head (camera)
            Vector3 forward = headTransform ? headTransform.forward : transform.forward;
            Vector3 right = headTransform ? headTransform.right : transform.right;

            if (flattenToGroundPlane)
            {
                forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(right, Vector3.up).normalized;
            }
            else
            {
                forward.Normalize();
                right.Normalize();
            }

            Vector3 worldMove = forward * move.y + right * move.x;
            moveTarget.position += worldMove * moveSpeed * Time.deltaTime;
        }

        void HandleSnapTurn()
        {
            if (snapTurnAction.action == null) return;

            if (moveTarget == null)
            {
                Debug.LogError("VrStickController: moveTarget is not set. Please assign the Transform to move in the Inspector.");
                return;
            }

            float raw = 0f;
            try
            {
                // XRI Snap Turn is often mapped to float or Vector2.x
                // InputAction itself has no valueType, so check from bound control
                var action = snapTurnAction.action;
                InputControl control = action.activeControl ?? (action.controls.Count > 0 ? action.controls[0] : null);
                Type ctrlType = control != null ? control.valueType : null;

                if (ctrlType == typeof(Vector2))
                {
                    raw = action.ReadValue<Vector2>().x;
                }
                else
                {
                    // Default is float (Axis)
                    raw = action.ReadValue<float>();
                }
            }
            catch { /* Do nothing on type mismatch */ }

            // Fire only on edge crossing the deadzone threshold + debounce
            float now = Time.time;
            bool risingRight = (_prevSnapValue <= snapTurnDeadzone) && (raw > snapTurnDeadzone);
            bool fallingLeft = (_prevSnapValue >= -snapTurnDeadzone) && (raw < -snapTurnDeadzone);

            if ((risingRight || fallingLeft) && (now - _lastSnapTime >= snapTurnDebounce))
            {
                float dir = risingRight ? +1f : -1f;
                // Rotate horizontally (Y axis in world space)
                moveTarget.Rotate(0f, snapTurnDegrees * dir, 0f, Space.World);
                _lastSnapTime = now;
            }

            _prevSnapValue = raw;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (headTransform == null && Camera.main != null)
                headTransform = Camera.main.transform;

            if (moveTarget == null)
            {
                Debug.LogWarning("VrStickController: moveTarget is not set. Please assign the Transform to move in the Inspector.");
            }

            moveSpeed = Mathf.Max(0f, moveSpeed);
            snapTurnDegrees = Mathf.Clamp(snapTurnDegrees, 1f, 180f);
            snapTurnDebounce = Mathf.Clamp(snapTurnDebounce, 0f, 2f);
        }
#endif
    }
}