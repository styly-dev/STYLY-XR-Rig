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
        private static readonly string packageIdentifier = "com.unity.xr.meta-openxr@2.3.0";

        private static void SetUpSdkSettings()
        {
            EditorApplication.delayCall += Step1;

            void Step1()  // Enable the OpenXR Loader
            {
                EnableXRPlugin(BuildTargetGroup.Android, "UnityEngine.XR.OpenXR.OpenXRLoader");

                EditorApplication.delayCall += Step2;
            }

            void Step2() // Enable the XR Feature Set
            {
                EnableXRFeatureSet(BuildTargetGroup.Android, "com.unity.openxr.featureset.meta");

                EditorApplication.delayCall += Step3;
            }

            void Step3() // Enable OpenXR Features
            {
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

                EditorApplication.delayCall += Step4;
            }

            void Step4() // Enable Interaction Profiles
            {
                EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
                {
                "com.unity.openxr.feature.input.handinteraction",
                "com.unity.openxr.feature.input.metaquestplus"
                });

                EditorApplication.delayCall += Step5;
            }

            void Step5() // Setup Other Settings
            {
                // Set Android Minimum API Level
                SetAndroidMinimumApiLevel(AndroidSdkVersions.AndroidApiLevel29);

                // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
                ApplyStylyPipelineAsset();

                // Use the new input system only
                UseNewInputSystemOnly();

                // Set graphics APIs
                SetGraphicsAPIs(BuildTarget.Android,
                    new List<GraphicsDeviceType> {
                    GraphicsDeviceType.Vulkan
                    });

                // Set OpenXR Render Mode
                SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Android);

                EditorApplication.delayCall += Step6;
            }

            void Step6() // Fix XR Project Validation Issues
            {
                XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

                EditorApplication.delayCall += Step7;
            }

            void Step7() // Additional Settings
            {
                // Nothing to do
            }

            // ==== Extra settings for XXXXXXXX ====

            // ToDo 1
            // Enable Passthrough
            // Add AR Camera Manager component
            // https://qiita.com/afjk/items/a57b07915feb0bed2d3a

        }

        #region CommonCode
        public static async void InstallPackage()
        {
            if (AddUnityPackage(packageIdentifier)) { SessionState.SetBool(packageIdentifier, true); }
            await WaitFramesAsync(1);
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
