using System.Linq;
using UnityEngine;

/// <summary>
/// This script attaches AR camera components to the Main Camera in the STYLY XR Rig prefab and configures occlusion settings based on user preferences.
/// </summary>
namespace Styly.XRRig
{
    public class SmartphoneARCameraManager : MonoBehaviour
    {
        /// <summary>
        /// Occlusion mode selection for AR camera
        /// </summary>
        private enum OcclusionMode
        {
            /// <summary>VR: Human Occlusion only, MR: Both Human and Environment Occlusion</summary>
            AutomaticForVRAndMR,
            /// <summary>Both Human and Environment Occlusion</summary>
            BothHumanAndEnvironment,
            /// <summary>Human Occlusion Only</summary>
            HumanOnly,
            /// <summary>Environment Occlusion Only</summary>
            EnvironmentOnly
        }

        /// <summary>
        /// Occlusion settings for AR Occlusion Manager
        /// </summary>
        public enum OcclusionSettings
        {
            Disabled,
            AutomaticForVR,
            AutomaticForMR,
            BothHumanAndEnvironment,
            HumanOnly,
            EnvironmentOnly
        }

        [Header("AR Camera Settings for smartphones")]
        [SerializeField] private OcclusionMode occlusionMode = OcclusionMode.AutomaticForVRAndMR;
        private UnityEngine.XR.ARFoundation.AROcclusionManager arOcclusionManager;

        void Start()
        {
#if !UNITY_EDITOR
                        AddOcclusionComponents();
                        InitializeOcclusionSettings();
#endif
        }

        /// <summary>
        /// Add AR Camera components to the Main Camera in STYLY XR Rig
        /// </summary>
        private void AddOcclusionComponents()
        {
            // Find the Main Camera in STYLY XR Rig
            var STYLYXRRig = GameObject.FindFirstObjectByType<Styly.XRRig.StylyXrRig>();
            var mainCamera = STYLYXRRig.GetComponentsInChildren<Camera>().FirstOrDefault(c => c.gameObject.name == "Main Camera");
            if (mainCamera != null)
            {
                // Add ARCameraManager component
                var arCameraManager = mainCamera.gameObject.GetOrAddComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();

                // Add ARCameraBackground component
                var arCameraBackground = mainCamera.gameObject.GetOrAddComponent<UnityEngine.XR.ARFoundation.ARCameraBackground>();

                // Add AROcclusionManager component
                arOcclusionManager = mainCamera.gameObject.GetOrAddComponent<UnityEngine.XR.ARFoundation.AROcclusionManager>();

                Debug.Log("AR Camera components are attached to Main Camera");
            }
            else
            {
                Debug.LogWarning("Main Camera not found in STYLY XR Rig");
            }
        }

        /// <summary>
        /// Initialize occlusion settings based on the selected occlusion mode
        /// </summary>
        private void InitializeOcclusionSettings()
        {
            var STYLYXRRig = GameObject.FindFirstObjectByType<Styly.XRRig.StylyXrRig>();
            bool passthroughMode = STYLYXRRig.PassthroughMode;

            switch (occlusionMode)
            {
                case OcclusionMode.AutomaticForVRAndMR:
                    ConfigureOcclusionSettings(passthroughMode ? OcclusionSettings.AutomaticForMR : OcclusionSettings.AutomaticForVR);
                    break;
                case OcclusionMode.BothHumanAndEnvironment:
                    ConfigureOcclusionSettings(OcclusionSettings.BothHumanAndEnvironment);
                    break;
                case OcclusionMode.HumanOnly:
                    ConfigureOcclusionSettings(OcclusionSettings.HumanOnly);
                    break;
                case OcclusionMode.EnvironmentOnly:
                    ConfigureOcclusionSettings(OcclusionSettings.EnvironmentOnly);
                    break;
            }
        }

        /// <summary>
        /// Configure AR Occlusion Manager settings based on the selected occlusion mode
        /// </summary>
        /// <param name="settings"></param>
        public void ConfigureOcclusionSettings(OcclusionSettings settings)
        {
#if !UNITY_EDITOR
            if (arOcclusionManager == null) { AddOcclusionComponents(); }

            switch (settings)
            {
                case OcclusionSettings.Disabled:
                    arOcclusionManager.requestedEnvironmentDepthMode = UnityEngine.XR.ARSubsystems.EnvironmentDepthMode.Disabled;
                    arOcclusionManager.requestedHumanStencilMode = UnityEngine.XR.ARSubsystems.HumanSegmentationStencilMode.Disabled;
                    arOcclusionManager.requestedHumanDepthMode = UnityEngine.XR.ARSubsystems.HumanSegmentationDepthMode.Disabled;
                    arOcclusionManager.requestedOcclusionPreferenceMode = UnityEngine.XR.ARSubsystems.OcclusionPreferenceMode.PreferHumanOcclusion;
                    break;

                case OcclusionSettings.AutomaticForVR:
                    arOcclusionManager.requestedEnvironmentDepthMode = UnityEngine.XR.ARSubsystems.EnvironmentDepthMode.Disabled;
                    arOcclusionManager.requestedHumanStencilMode = UnityEngine.XR.ARSubsystems.HumanSegmentationStencilMode.Best;
                    arOcclusionManager.requestedHumanDepthMode = UnityEngine.XR.ARSubsystems.HumanSegmentationDepthMode.Best;
                    arOcclusionManager.requestedOcclusionPreferenceMode = UnityEngine.XR.ARSubsystems.OcclusionPreferenceMode.PreferHumanOcclusion;
                    break;

                case OcclusionSettings.AutomaticForMR:
                    arOcclusionManager.requestedEnvironmentDepthMode = UnityEngine.XR.ARSubsystems.EnvironmentDepthMode.Best;
                    arOcclusionManager.requestedHumanStencilMode = UnityEngine.XR.ARSubsystems.HumanSegmentationStencilMode.Best;
                    arOcclusionManager.requestedHumanDepthMode = UnityEngine.XR.ARSubsystems.HumanSegmentationDepthMode.Best;
                    arOcclusionManager.requestedOcclusionPreferenceMode = UnityEngine.XR.ARSubsystems.OcclusionPreferenceMode.PreferHumanOcclusion;
                    break;

                case OcclusionSettings.BothHumanAndEnvironment:
                    arOcclusionManager.requestedEnvironmentDepthMode = UnityEngine.XR.ARSubsystems.EnvironmentDepthMode.Best;
                    arOcclusionManager.requestedHumanStencilMode = UnityEngine.XR.ARSubsystems.HumanSegmentationStencilMode.Best;
                    arOcclusionManager.requestedHumanDepthMode = UnityEngine.XR.ARSubsystems.HumanSegmentationDepthMode.Best;
                    arOcclusionManager.requestedOcclusionPreferenceMode = UnityEngine.XR.ARSubsystems.OcclusionPreferenceMode.PreferHumanOcclusion;
                    break;

                case OcclusionSettings.HumanOnly:
                    arOcclusionManager.requestedEnvironmentDepthMode = UnityEngine.XR.ARSubsystems.EnvironmentDepthMode.Disabled;
                    arOcclusionManager.requestedHumanStencilMode = UnityEngine.XR.ARSubsystems.HumanSegmentationStencilMode.Best;
                    arOcclusionManager.requestedHumanDepthMode = UnityEngine.XR.ARSubsystems.HumanSegmentationDepthMode.Best;
                    arOcclusionManager.requestedOcclusionPreferenceMode = UnityEngine.XR.ARSubsystems.OcclusionPreferenceMode.PreferHumanOcclusion;
                    break;
                case OcclusionSettings.EnvironmentOnly:
                    arOcclusionManager.requestedEnvironmentDepthMode = UnityEngine.XR.ARSubsystems.EnvironmentDepthMode.Best;
                    arOcclusionManager.requestedHumanStencilMode = UnityEngine.XR.ARSubsystems.HumanSegmentationStencilMode.Disabled;
                    arOcclusionManager.requestedHumanDepthMode = UnityEngine.XR.ARSubsystems.HumanSegmentationDepthMode.Disabled;
                    arOcclusionManager.requestedOcclusionPreferenceMode = UnityEngine.XR.ARSubsystems.OcclusionPreferenceMode.PreferEnvironmentOcclusion;
                    break;
            }

            // Requesting environment depth without android.permission.SCENE_UNDERSTANDING_FINE granted
            // (or on a subsystem that doesn't support it) makes xrCreateDepthSwapchainANDROID fail with
            // XR_ERROR_PERMISSION_INSUFFICIENT on Android XR, which crashes the whole OpenXR session a
            // few seconds later. Fall back to Human-only occlusion instead of requesting it blindly.
            if (arOcclusionManager.requestedEnvironmentDepthMode != UnityEngine.XR.ARSubsystems.EnvironmentDepthMode.Disabled
                && !CanUseEnvironmentDepth())
            {
                Debug.LogWarning("Environment depth occlusion was requested but is unavailable (missing " +
                    $"{RequestAndroidXRPermissions.SceneUnderstandingFine} permission, or unsupported by " +
                    "this device) - falling back to Human occlusion only.");
                arOcclusionManager.requestedEnvironmentDepthMode = UnityEngine.XR.ARSubsystems.EnvironmentDepthMode.Disabled;
                arOcclusionManager.requestedHumanStencilMode = UnityEngine.XR.ARSubsystems.HumanSegmentationStencilMode.Best;
                arOcclusionManager.requestedHumanDepthMode = UnityEngine.XR.ARSubsystems.HumanSegmentationDepthMode.Best;
                arOcclusionManager.requestedOcclusionPreferenceMode = UnityEngine.XR.ARSubsystems.OcclusionPreferenceMode.PreferHumanOcclusion;
            }
#endif
        }

#if !UNITY_EDITOR
        /// <summary>
        /// Whether environment depth occlusion can actually be requested: the Android XR runtime
        /// permission must be granted, and the occlusion subsystem must not have already reported
        /// it as unsupported (Unknown is allowed through since the subsystem may not have started
        /// yet at this point in the lifecycle).
        /// </summary>
        private bool CanUseEnvironmentDepth()
        {
            if (!RequestAndroidXRPermissions.IsGranted(RequestAndroidXRPermissions.SceneUnderstandingFine))
            {
                return false;
            }

            return arOcclusionManager.descriptor?.environmentDepthImageSupported
                != UnityEngine.XR.ARSubsystems.Supported.Unsupported;
        }
#endif
    }
}
