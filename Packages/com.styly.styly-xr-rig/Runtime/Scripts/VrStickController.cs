using System;
using System.Reflection;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Styly.XRRig
{
    public class VrStickController : MonoBehaviour
    {
        private const string MoveTargetNotAvailableError = "VrStickController: No movement target is available. Enable a Main Camera for physical movement, or add STYLY XR Rig for virtual movement.";

        [Header("Input Actions (assign in Inspector)")]
        [Tooltip("Assign \"XRI Left Locomotion/Move\" from Starter Assets/XRI Default Input Actions")]
        public InputActionProperty moveAction;

        [Tooltip("Assign \"XRI Right Locomotion/Snap Turn\" from Starter Assets/XRI Default Input Actions")]
        public InputActionProperty snapTurnAction;

        [Tooltip("Assign an Input Action that fires when the vertical up button is pressed")]
        public InputActionProperty verticalUpAction;

        [Tooltip("Assign an Input Action that fires when the vertical down button is pressed")]
        public InputActionProperty verticalDownAction;

        // Head (Main Camera) will be assigned
        private Transform headTransform;

        // STYLY XR Rig root will be assigned
        private Transform moveTarget;

        [SerializeField, InspectorName("Move Physical Transform")]
        private bool movePhysicalTransform = true;

        private Transform physicalTarget;
        private XROrigin physicalXrOrigin;
        private bool netSyncReflectionResolved;
        private Type netSyncManagerType;
        private PropertyInfo netSyncInstanceProperty;
        private FieldInfo netSyncXrOriginTransformField;
        private FieldInfo netSyncPhysicalOffsetPositionField;
        private FieldInfo netSyncPhysicalOffsetRotationField;
        private bool hasLoggedNetSyncRebaseError;

        [Tooltip("Move speed (m/s)")]
        public float moveSpeed = 2.0f;

        [Tooltip("Ignore Y component and move along the horizontal plane")]
        private bool flattenToGroundPlane = true;

        [Header("Snap Turn")]
        [Tooltip("Snap turn angle per step (degrees)")]
        public float snapTurnDegrees = 45f;

        [Tooltip("Snap turn input threshold (right: +threshold / left: -threshold)")]
        [Range(0.1f, 0.95f)]
        private float snapTurnDeadzone = 0.5f;

        [Tooltip("Minimum interval between consecutive snap turns in the same direction (seconds)")]
        private float snapTurnDebounce = 0.25f;

        // Internal state
        float _prevSnapValue = 0f;
        float _lastSnapTime = -999f;
        private bool hasLoggedMoveTargetError = false;

        private const float ButtonPressedThreshold = 0.5f;


        void Start()
        {
            var stylyXrRig = FindFirstObjectByType<StylyXrRig>();
            if (stylyXrRig != null)
            {
                moveTarget = stylyXrRig.transform;
            }

            ResolveCameraTargets();

            if (!HasActiveMoveTarget())
            {
                LogMoveTargetErrorOnce();
            }
        }

        void OnEnable()
        {
            // Enable InputActions (generally safe even with Input Action Manager)
            if (moveAction.action != null) moveAction.action.Enable();
            if (snapTurnAction.action != null) snapTurnAction.action.Enable();
            if (verticalUpAction.action != null) verticalUpAction.action.Enable();
            if (verticalDownAction.action != null) verticalDownAction.action.Enable();
        }

        void OnDisable()
        {
            if (moveAction.action != null) moveAction.action.Disable();
            if (snapTurnAction.action != null) snapTurnAction.action.Disable();
            if (verticalUpAction.action != null) verticalUpAction.action.Disable();
            if (verticalDownAction.action != null) verticalDownAction.action.Disable();
        }

        void Update()
        {
            HandleMove();
            HandleVerticalMove();
            HandleSnapTurn();
        }

        void HandleMove()
        {
            if (moveAction.action == null) return;

            if (!HasActiveMoveTarget())
            {
                LogMoveTargetErrorOnce();
                return;
            }

            Vector2 move = Vector2.zero;
            try
            {
                move = moveAction.action.ReadValue<Vector2>();
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning($"VrStickController: Failed to read move input as Vector2: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"VrStickController: Unexpected error reading move input: {ex.Message}");
            }

            if (move.sqrMagnitude < 0.0001f) return;

            // Forward and right vectors based on head (camera)
            Vector3 forward = headTransform != null ? headTransform.forward : transform.forward;
            Vector3 right = headTransform != null ? headTransform.right : transform.right;

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
            ApplyWorldMovement(worldMove * moveSpeed * Time.deltaTime);
        }

        void HandleVerticalMove()
        {
            bool hasUpAction = verticalUpAction.action != null;
            bool hasDownAction = verticalDownAction.action != null;

            if (!hasUpAction && !hasDownAction) return;

            if (!HasActiveMoveTarget())
            {
                LogMoveTargetErrorOnce();
                return;
            }

            float upValue = 0f;
            if (hasUpAction)
            {
                try
                {
                    upValue = verticalUpAction.action.ReadValue<float>();
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogWarning($"VrStickController: Failed to read vertical up input: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"VrStickController: Unexpected error reading vertical up input: {ex.Message}");
                }
            }

            float downValue = 0f;
            if (hasDownAction)
            {
                try
                {
                    downValue = verticalDownAction.action.ReadValue<float>();
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogWarning($"VrStickController: Failed to read vertical down input: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"VrStickController: Unexpected error reading vertical down input: {ex.Message}");
                }
            }

            bool upPressed = hasUpAction && upValue > ButtonPressedThreshold;
            bool downPressed = hasDownAction && downValue > ButtonPressedThreshold;

            if (!upPressed && !downPressed) return;

            Vector3 verticalDirection = Vector3.zero;
            if (upPressed)
            {
                verticalDirection += Vector3.up;
            }

            if (downPressed)
            {
                verticalDirection += Vector3.down;
            }

            if (verticalDirection.sqrMagnitude < 0.0001f) return;

            verticalDirection.Normalize();
            ApplyWorldMovement(verticalDirection * moveSpeed * Time.deltaTime);
        }

        void HandleSnapTurn()
        {
            if (snapTurnAction.action == null) return;

            if (!HasActiveMoveTarget())
            {
                LogMoveTargetErrorOnce();
                return;
            }

            float raw = 0f;
            try
            {
                // XRI Snap Turn is often mapped to float or Vector2.x
                // InputAction itself has no valueType, so check from bound control
                var action = snapTurnAction.action;
                InputControl control = action.activeControl;
                if (control == null && action.controls.Count > 0)
                {
                    control = action.controls[0];
                }
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
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning($"VrStickController: Failed to read snap turn input: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"VrStickController: Unexpected error reading snap turn input: {ex.Message}");
            }

            // Fire only on edge crossing the deadzone threshold + debounce
            float now = Time.time;
            bool risingRight = (_prevSnapValue <= snapTurnDeadzone) && (raw > snapTurnDeadzone);
            bool fallingLeft = (_prevSnapValue >= -snapTurnDeadzone) && (raw < -snapTurnDeadzone);

            if ((risingRight || fallingLeft) && (now - _lastSnapTime >= snapTurnDebounce))
            {
                float dir = risingRight ? +1f : -1f;
                ApplyYawRotation(snapTurnDegrees * dir);
                _lastSnapTime = now;
            }

            _prevSnapValue = raw;
        }

        private void ResolveCameraTargets()
        {
            if (headTransform == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    headTransform = mainCamera.transform;
                }
            }

            if (physicalTarget == null)
            {
                physicalTarget = ResolvePhysicalTarget();
            }
        }

        private Transform ResolvePhysicalTarget()
        {
            // NetSync observes the XR Origin locomotion delta, so prefer the origin over Camera Offset.
            if (physicalXrOrigin == null && headTransform != null)
            {
                physicalXrOrigin = headTransform.GetComponentInParent<XROrigin>();
            }

            if (physicalXrOrigin == null)
            {
                physicalXrOrigin = FindFirstObjectByType<XROrigin>();
            }

            if (physicalXrOrigin != null)
            {
                Transform xrOriginTransform = physicalXrOrigin.transform;
                if (xrOriginTransform != null)
                {
                    return xrOriginTransform;
                }
            }

            if (moveTarget != null)
            {
                return moveTarget;
            }

            if (headTransform != null)
            {
                return headTransform.parent != null ? headTransform.parent : headTransform;
            }

            return null;
        }

        private bool HasActiveMoveTarget()
        {
            if (movePhysicalTransform)
            {
                ResolveCameraTargets();
                if (physicalTarget != null)
                {
                    return true;
                }
            }

            return moveTarget != null;
        }

        private void LogMoveTargetErrorOnce()
        {
            if (hasLoggedMoveTargetError) return;

            Debug.LogError(MoveTargetNotAvailableError);
            hasLoggedMoveTargetError = true;
        }

        private void ApplyWorldMovement(Vector3 worldDelta)
        {
            if (movePhysicalTransform)
            {
                ResolveCameraTargets();
                if (physicalTarget != null)
                {
                    if (physicalXrOrigin != null)
                    {
                        Camera xrCamera = physicalXrOrigin.Camera;
                        if (xrCamera != null)
                        {
                            Vector3 desiredCameraPosition = xrCamera.transform.position + worldDelta;
                            if (physicalXrOrigin.MoveCameraToWorldLocation(desiredCameraPosition))
                            {
                                RebaseNetSyncPhysicalOrigin();
                                return;
                            }
                        }
                    }

                    MoveTransformByWorldDelta(physicalTarget, worldDelta);
                    RebaseNetSyncPhysicalOrigin();
                    return;
                }
            }

            if (moveTarget != null)
            {
                moveTarget.position += worldDelta;
            }
        }

        private static void MoveTransformByWorldDelta(Transform targetTransform, Vector3 worldDelta)
        {
            if (targetTransform == null) return;

            Transform parent = targetTransform.parent;
            if (parent != null)
            {
                targetTransform.localPosition += parent.InverseTransformVector(worldDelta);
                return;
            }

            targetTransform.position += worldDelta;
        }

        private void ApplyYawRotation(float degrees)
        {
            if (movePhysicalTransform)
            {
                ResolveCameraTargets();
                if (physicalTarget != null)
                {
                    if (physicalXrOrigin != null && physicalXrOrigin.RotateAroundCameraUsingOriginUp(degrees))
                    {
                        RebaseNetSyncPhysicalOrigin();
                        return;
                    }

                    Quaternion desiredWorldRotation = Quaternion.Euler(0f, degrees, 0f) * physicalTarget.rotation;
                    SetWorldRotationAsLocal(physicalTarget, desiredWorldRotation);
                    RebaseNetSyncPhysicalOrigin();
                    return;
                }
            }

            if (moveTarget != null)
            {
                // Rotate horizontally (Y axis in world space)
                moveTarget.Rotate(0f, degrees, 0f, Space.World);
            }
        }

        private static void SetWorldRotationAsLocal(Transform targetTransform, Quaternion worldRotation)
        {
            if (targetTransform == null) return;

            Transform parent = targetTransform.parent;
            if (parent != null)
            {
                targetTransform.localRotation = Quaternion.Inverse(parent.rotation) * worldRotation;
                return;
            }

            targetTransform.localRotation = worldRotation;
        }

        private void RebaseNetSyncPhysicalOrigin()
        {
            if (physicalTarget == null) return;
            if (!TryGetNetSyncManager(out object netSyncManager)) return;

            try
            {
                if (netSyncXrOriginTransformField != null && physicalXrOrigin != null)
                {
                    Transform xrOriginTransform = physicalXrOrigin.transform;
                    if (xrOriginTransform != null)
                    {
                        netSyncXrOriginTransformField.SetValue(netSyncManager, xrOriginTransform);
                    }
                }

                netSyncPhysicalOffsetPositionField.SetValue(netSyncManager, physicalTarget.position);
                netSyncPhysicalOffsetRotationField.SetValue(netSyncManager, physicalTarget.eulerAngles);
            }
            catch (Exception ex)
            {
                if (!hasLoggedNetSyncRebaseError)
                {
                    Debug.LogWarning($"VrStickController: Failed to rebase STYLY NetSync physical origin: {ex.Message}");
                    hasLoggedNetSyncRebaseError = true;
                }
            }
        }

        private bool TryGetNetSyncManager(out object netSyncManager)
        {
            netSyncManager = null;
            if (!EnsureNetSyncReflection()) return false;

            object instance = netSyncInstanceProperty.GetValue(null, null);
            UnityEngine.Object unityInstance = instance as UnityEngine.Object;
            if (unityInstance == null) return false;

            netSyncManager = instance;
            return true;
        }

        private bool EnsureNetSyncReflection()
        {
            if (netSyncReflectionResolved)
            {
                return netSyncManagerType != null
                    && netSyncInstanceProperty != null
                    && netSyncPhysicalOffsetPositionField != null
                    && netSyncPhysicalOffsetRotationField != null;
            }

            netSyncReflectionResolved = true;
            netSyncManagerType = ResolveNetSyncManagerType();
            if (netSyncManagerType == null) return false;

            BindingFlags staticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            netSyncInstanceProperty = netSyncManagerType.GetProperty("Instance", staticFlags);

            BindingFlags instanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            netSyncXrOriginTransformField = netSyncManagerType.GetField("_XrOriginTransform", instanceFlags);
            netSyncPhysicalOffsetPositionField = netSyncManagerType.GetField("_physicalOffsetPosition", instanceFlags);
            netSyncPhysicalOffsetRotationField = netSyncManagerType.GetField("_physicalOffsetRotation", instanceFlags);

            return netSyncInstanceProperty != null
                && netSyncPhysicalOffsetPositionField != null
                && netSyncPhysicalOffsetRotationField != null;
        }

        private static Type ResolveNetSyncManagerType()
        {
            Type managerType = Type.GetType("Styly.NetSync.NetSyncManager, com.styly.styly-netsync");
            if (managerType != null)
            {
                return managerType;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                managerType = assemblies[i].GetType("Styly.NetSync.NetSyncManager");
                if (managerType != null)
                {
                    return managerType;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            ResolveCameraTargets();

            moveSpeed = Mathf.Max(0f, moveSpeed);
            snapTurnDegrees = Mathf.Clamp(snapTurnDegrees, 1f, 180f);
            snapTurnDebounce = Mathf.Clamp(snapTurnDebounce, 0f, 2f);
        }
#endif
    }
}
