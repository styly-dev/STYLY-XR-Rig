using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SdkSwitcher.SwitchSdkUtils;

namespace Styly.XRRig.SdkSwitcher
{
    public class SwitchSdk_PicoUnityOpenXrSdk
    {
        private static readonly string packageIdentifier = "https://github.com/Pico-Developer/PICO-Unity-OpenXR-SDK.git#release_1.4.0";

        public static void InstallPackage()
        {
            if (AddUnityPackage(packageIdentifier)) { SessionState.SetBool(packageIdentifier, true); }
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (!SessionState.GetBool(packageIdentifier, false)) { return; }
            SessionState.EraseBool(packageIdentifier);
            SetUpSdkSettings();
        }

        private static void SetUpSdkSettings()
        {
            // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
            ApplyStylyPipelineAsset();

            // Use the new input system only
            UseNewInputSystemOnly();

           // Set graphics APIs to Vulkan and OpenGLES3
            SetGraphicsAPIs(BuildTarget.Android,
                new List<GraphicsDeviceType> {
                    GraphicsDeviceType.Vulkan,
                    GraphicsDeviceType.OpenGLES3
                });

            // Enable the OpenXR Loader and set the XR Feature Set
            EnableXRPlugin(BuildTargetGroup.Android, typeof(OpenXRLoader));
            EnableXRFeatureSet(BuildTargetGroup.Android, "com.picoxr.openxr.features");

            // Enable OpenXR Features
            EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
            {
                "com.unity.openxr.feature.input.handtracking",
                "com.pico.openxr.feature.passthrough"
            });

            // Enable Interaction Profiles
            EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
            {
                "com.unity.openxr.feature.input.handinteraction",
                "com.unity.openxr.feature.input.PICO4touch",
                "com.unity.openxr.feature.input.PICO4Ultratouch"
            });

            // Set OpenXR Render Mode to MultiPass
            SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Android);

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

            // ==== Extra settings for PICO XR ==== 

            // Set isCameraSubsystem to true
            SetFieldValueOfOpenXrFeature(BuildTargetGroup.Android, "com.pico.openxr.feature.passthrough", "isCameraSubsystem", true);

            // Configure PICO Hand Tracking
            SwitchSdkUtils.ConfigurePicoHandTracking();
        }
    }
}
