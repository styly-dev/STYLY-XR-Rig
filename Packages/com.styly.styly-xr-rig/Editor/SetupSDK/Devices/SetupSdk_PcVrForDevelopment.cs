using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdk_PcVrForDevelopment
    {
        private static readonly string packageIdentifier = null;

        private static async void SetUpSdkSettings()
        {
            // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
            ApplyStylyPipelineAsset();

            // Use the new input system only
            UseNewInputSystemOnly();

            // // Set graphics APIs to Vulkan and OpenGLES3
            // SetGraphicsAPIs(BuildTarget.Android,
            //     new List<GraphicsDeviceType> {
            //         GraphicsDeviceType.Vulkan,
            //         GraphicsDeviceType.OpenGLES3
            //     });

            // Enable the OpenXR Loader and set the XR Feature Set
            EnableXRPlugin(BuildTargetGroup.Standalone, typeof(OpenXRLoader));
            // EnableXRFeatureSet(BuildTargetGroup.Standalone, "com.xxxx.xxxx.features");

            // Wait for 1 frame to ensure the OpenXR Loader is initialized
            await WaitFramesAsync(1);
            
            // Enable OpenXR Features
            EnableOpenXrFeatures(BuildTargetGroup.Standalone, new string[]
            {
                "com.unity.openxr.feature.input.handtracking",
                "com.unity.openxr.feature.compositionlayers"
            });

            // Enable Interaction Profiles
            EnableInteractionProfiles(BuildTargetGroup.Standalone, new string[]
            {
                "com.unity.openxr.feature.input.handinteraction",
                "com.unity.openxr.feature.input.khrsimpleprofile"
            });

            // Set OpenXR Render Mode to MultiPass
            SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Standalone);

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Standalone);

            // ==== Extra settings for PCVR ====


        }

        #region CommonCode
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
        #endregion
    }
}
