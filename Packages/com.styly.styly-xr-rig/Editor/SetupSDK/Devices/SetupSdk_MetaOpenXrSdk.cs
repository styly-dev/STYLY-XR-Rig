using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdk_MetaOpenXrSdk
    {
        /// The package identifier for SDK (UPM package name@version, gitURL etc).
        private static readonly string packageIdentifier = "com.unity.xr.meta-openxr@2.2.0";

        private static void SetUpSdkSettings()
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
            EnableXRFeatureSet(BuildTargetGroup.Android, "com.unity.openxr.featureset.meta");

            // Enable OpenXR Features
            EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
            {
                "com.unity.openxr.feature.input.handtracking",
                "com.unity.openxr.feature.metaquest",
                "com.unity.openxr.feature.arfoundation-meta-anchor",
                "com.unity.openxr.feature.meta-boundary-visibility",
                "com.unity.openxr.feature.arfoundation-meta-bounding-boxes",
                "com.unity.openxr.feature.arfoundation-meta-session",
                "com.unity.openxr.feature.meta-colocation-discovery",
                "com.unity.openxr.feature.arfoundation-meta-camera"
            });

            // Enable Interaction Profiles
            EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
            {
                "com.unity.openxr.feature.input.handinteraction",
                "com.unity.openxr.feature.input.metaquestplus"
            });

            // Set OpenXR Render Mode to MultiPass
            SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Android);

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

            // ==== Extra settings for XXXXXXXX ====


        }

#region CommonCode
        public static void InstallPackage()
        {
            // Attempt to add the Unity package and handle the result
            int PackageInstallResult = AddUnityPackage(packageIdentifier);
            switch (PackageInstallResult)
            {
                case 0: // Package already installed, continue setting up SDK settings
                    SetUpSdkSettings();
                    break;
                case 1: // Package added successfully, set up SDK settings after scripts reload
                    SessionState.SetBool(packageIdentifier, true);
                    break;
                case -1: // Error occurred while adding the package
                    break;
            }
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
