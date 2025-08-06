using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdk_AndroidXR
    {
        private static readonly string packageIdentifier = "com.unity.xr.androidxr-openxr@1.0.1";

        private static async void SetUpSdkSettings()
        {
            // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
            ApplyStylyPipelineAsset();

            // Use the new input system only
            UseNewInputSystemOnly();

            // Set graphics APIs to Vulkan and OpenGLES3
            SetGraphicsAPIs(BuildTarget.Android,
                new List<GraphicsDeviceType> {
                    GraphicsDeviceType.Vulkan
                });

            // Enable the OpenXR Loader and set the XR Feature Set
            EnableXRPlugin(BuildTargetGroup.Android, typeof(OpenXRLoader));
            EnableXRFeatureSet(BuildTargetGroup.Android, "com.unity.openxr.featureset.android");

            // Wait for 1 frame to ensure the OpenXR Loader is initialized
            await WaitFramesAsync(1);

            // Enable OpenXR Features
            EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
            {
                "com.unity.openxr.feature.androidxr-support",
                "com.unity.openxr.feature.arfoundation-androidxr-anchor",
                "com.unity.openxr.feature.arfoundation-androidxr-camera",
                "com.unity.openxr.feature.arfoundation-androidxr-face",
                "com.unity.openxr.feature.arfoundation-androidxr-occlusion",
                "com.unity.openxr.feature.arfoundation-androidxr-plane",
                "com.unity.openxr.feature.arfoundation-androidxr-raycast",
                "com.unity.openxr.feature.arfoundation-androidxr-session",
                "com.unity.openxr.feature.androidxr-display-utilities",
                "com.unity.openxr.feature.androidxr-hand-mesh-data"
            });

            // Enable Interaction Profiles
            EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
            {
                "com.unity.openxr.feature.input.handinteraction",
                "com.unity.openxr.feature.input.khrsimpleprofile"
            });

            // Set OpenXR Render Mode to MultiPass
            SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Android);

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

            // ==== Extra settings for Android XR ====

            // ToDo 1
            // [Android XR: AR Camera] AR Camera Manager component should be enabled for Passthrough to function correctly.
            // https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.0/manual/features/camera.html

            // ToDo 2
            // For functionalities not provided by the above package, Google offers additional extensions.
            // https://github.com/android/android-xr-unity-package

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
