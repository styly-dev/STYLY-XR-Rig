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
        private OcclusionSettings requestedSettings;
        private bool hasRequestedSettings;

        void OnEnable()
        {
            RequestAndroidXRPermissions.Granted += OnPermissionGranted;
            if (hasRequestedSettings) { ApplyOcclusionSettings(requestedSettings); }
        }

        void OnDisable()
        {
            RequestAndroidXRPermissions.Granted -= OnPermissionGranted;
            if (RequestAndroidXRPermissions.IsAndroidXR && arOcclusionManager != null)
            {
                arOcclusionManager.enabled = false;
            }
        }

        private void OnPermissionGranted(string permission)
        {
            if (permission == RequestAndroidXRPermissions.SceneUnderstandingFine && hasRequestedSettings)
            {
                ApplyOcclusionSettings(requestedSettings);
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            // Also handle permission changes made in Android settings while the app was suspended.
            if (hasFocus && isActiveAndEnabled && hasRequestedSettings && RequestAndroidXRPermissions.IsAndroidXR)
            {
                ApplyOcclusionSettings(requestedSettings);
            }
        }

        void Start()
        {
#if !UNITY_EDITOR
            InitializeOcclusionSettings();
#endif
        }

        /// <summary>
        /// Add AR Camera components to the Main Camera in STYLY XR Rig
        /// </summary>
        private void AddOcclusionComponents()
        {
            // Find the Main Camera in STYLY XR Rig
            var mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
            {
                var rig = GameObject.FindFirstObjectByType<StylyXrRig>();
                if (rig != null)
                {
                    mainCamera = rig.GetComponentsInChildren<Camera>().FirstOrDefault(c => c.gameObject.name == "Main Camera");
                }
            }
            if (mainCamera != null)
            {
                // Add ARCameraManager component
                var arCameraManager = mainCamera.gameObject.GetOrAddComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();

                // Add ARCameraBackground component
                var arCameraBackground = mainCamera.gameObject.GetOrAddComponent<UnityEngine.XR.ARFoundation.ARCameraBackground>();

                arOcclusionManager = mainCamera.GetComponent<UnityEngine.XR.ARFoundation.AROcclusionManager>();
                // AddComponent immediately invokes OnEnable on an active camera. Delay creation
                // until permission is granted; disabling the component afterwards is too late.
                if (arOcclusionManager == null && ShouldEnableOcclusion())
                {
                    arOcclusionManager = mainCamera.gameObject.AddComponent<UnityEngine.XR.ARFoundation.AROcclusionManager>();
                }

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
            ApplyOcclusionSettings(settings);
#endif
        }

        internal void ApplyOcclusionSettings(OcclusionSettings settings)
        {
            requestedSettings = settings;
            hasRequestedSettings = true;
            RequestAndroidXRPermissions.RequestPermissions();
            if (arOcclusionManager == null) { AddOcclusionComponents(); }
            if (arOcclusionManager == null) { return; }

            if (RequestAndroidXRPermissions.IsAndroidXR)
            {
                // Android XR 1.4.1 ignores requestedEnvironmentDepthMode. Control the
                // subsystem lifetime instead, including when switching from MR to VR.
                arOcclusionManager.enabled = ShouldEnableOcclusion();
                if (!arOcclusionManager.enabled) { return; }
            }

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
        }

        private bool ShouldEnableOcclusion()
        {
            if (!RequestAndroidXRPermissions.IsAndroidXR) { return true; }

            bool needsEnvironmentDepth = requestedSettings == OcclusionSettings.AutomaticForMR
                || requestedSettings == OcclusionSettings.BothHumanAndEnvironment
                || requestedSettings == OcclusionSettings.EnvironmentOnly;
            return needsEnvironmentDepth
                && RequestAndroidXRPermissions.IsGranted(RequestAndroidXRPermissions.SceneUnderstandingFine);
        }
    }
}
