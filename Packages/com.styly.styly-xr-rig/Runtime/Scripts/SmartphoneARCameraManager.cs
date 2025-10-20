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
#endif
        }
    }
}
