using System;
using System.Collections.Generic;
using Unity.XR.PICO.TOBSupport;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;

// This optional assembly is reached through startup registration, not scene references.
[assembly: UnityEngine.Scripting.AlwaysLinkAssembly]

namespace Styly.XRRig
{
    internal sealed class PicoLbeRecenterBackend : IPicoLbeRecenterBackend
    {
        private readonly List<XRInputSubsystem> inputs = new List<XRInputSubsystem>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            PicoLbeStartupRecenter.BackendFactory = () => new PicoLbeRecenterBackend();
#endif
        }

        public bool IsPicoRuntime =>
            !string.IsNullOrEmpty(OpenXRRuntime.name)
            && OpenXRRuntime.name.IndexOf("Pico", StringComparison.OrdinalIgnoreCase) >= 0;

        public bool IsTrackingReady
        {
            get
            {
                if (!Application.isFocused || !OpenXRSettings.AllowRecentering)
                    return false;

                inputs.Clear();
                SubsystemManager.GetSubsystems(inputs);
                bool floorRunning = false;
                foreach (XRInputSubsystem input in inputs)
                {
                    if (input.running && input.GetTrackingOriginMode() == TrackingOriginModeFlags.Floor)
                    {
                        floorRunning = true;
                        break;
                    }
                }
                if (!floorRunning)
                    return false;

                InputDevice head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
                const InputTrackingState pose = InputTrackingState.Position | InputTrackingState.Rotation;
                return head.isValid
                    && head.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) && tracked
                    && head.TryGetFeatureValue(CommonUsages.trackingState, out InputTrackingState state) && (state & pose) == pose
                    && head.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position) && IsFinite(position)
                    && head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation) && IsFinite(rotation);
            }
        }

        public bool Initialize() => PXR_Enterprise.InitEnterpriseService();
        public void Bind(Action<bool> completed) => PXR_Enterprise.BindEnterpriseService(completed);
        public void QueryLbe(Action<string> completed) => PXR_Enterprise.GetSwitchLargeSpaceStatus(completed);
        public int Recenter() => PXR_Enterprise.Recenter();

        // Enterprise Service is shared with the application; never unbind it here.
        private static bool IsFinite(Vector3 value) =>
            IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);

        private static bool IsFinite(Quaternion value) =>
            IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z) && IsFinite(value.w)
            && value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w > 0.0001f;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
