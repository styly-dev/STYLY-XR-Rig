using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdk_Template
    {
        private static readonly string packageIdentifier = "com.xxxxx.xxxxxx@1.0.0";

        private static void SetUpSdkSettings()
        {
            EditorApplication.delayCall += Step1;

            void Step1()  // Enable the OpenXR Loader
            {
                EnableXRPlugin(BuildTargetGroup.Android, typeof(OpenXRLoader));

                EditorApplication.delayCall += Step2;
            }

            void Step2() // Enable the XR Feature Set
            {
                EnableXRFeatureSet(BuildTargetGroup.Android, "com.xxxx.xxxx.features");

                EditorApplication.delayCall += Step3;
            }

            void Step3() // Enable OpenXR Features
            {
                EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
                {
                "com.unity.openxr.feature.input.handtracking",
                "com.xxxx.xxxx.feature.passthrough"
                });

                EditorApplication.delayCall += Step4;
            }

            void Step4() // Enable Interaction Profiles
            {
                EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
                {
                "com.unity.openxr.feature.input.handinteraction",
                "com.unity.openxr.feature.input.xxxxxxxx",
                "com.unity.openxr.feature.input.yyyyyyyy"
                });

                EditorApplication.delayCall += Step5;
            }

            void Step5() // Setup Other Settings
            {
                // Set Android Minimum API Level
                SetAndroidMinimumApiLevel(AndroidSdkVersions.AndroidApiLevel23);

                // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
                ApplyStylyPipelineAsset();

                // Use the new input system only
                UseNewInputSystemOnly();

                // Set graphics APIs
                SetGraphicsAPIs(BuildTarget.Android,
                    new List<GraphicsDeviceType> {
                    GraphicsDeviceType.Vulkan,
                    GraphicsDeviceType.OpenGLES3
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
        }

        #region CommonCode
        public static async void InstallPackage()
        {
            PrepareSdkInstallation();
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
