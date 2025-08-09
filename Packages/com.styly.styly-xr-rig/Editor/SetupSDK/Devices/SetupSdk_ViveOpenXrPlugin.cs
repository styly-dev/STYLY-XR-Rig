using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;
using System.Threading.Tasks;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdk_ViveOpenXrPlugin
    {
        private static readonly string packageIdentifier = "com.htc.upm.vive.openxr@2.5.1";

        private static async void SetUpSdkSettings()
        {
            // Set Android Minimum API Level
            SetAndroidMinimumApiLevel(AndroidSdkVersions.AndroidApiLevel29);

            // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
            ApplyStylyPipelineAsset();

            // Use the new input system only
            UseNewInputSystemOnly();

            // Set graphics APIs to OpenGLES3
            SetGraphicsAPIs(BuildTarget.Android,
                new List<GraphicsDeviceType> {
                    GraphicsDeviceType.OpenGLES3
                });

            // Enable the OpenXR Loader and set the XR Feature Set
            EnableXRPlugin(BuildTargetGroup.Android, typeof(OpenXRLoader));
            EnableXRFeatureSet(BuildTargetGroup.Android, "com.htc.vive.openxr.featureset.vivexr");

            // Wait for 1 frame to ensure the OpenXR Loader is initialized
            await WaitFramesAsync(1);

            // Enable OpenXR Features
            EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
            {
                "vive.openxr.feature.compositionlayer",
                "vive.openxr.feature.hand.tracking",
                "vive.openxr.feature.passthrough",
                "com.unity.openxr.feature.vivefocus3"
            });

            // Enable Interaction Profiles
            EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
            {
                "com.unity.openxr.feature.input.handinteraction",
                "vive.openxr.feature.focus3controller"
            });

            // Set OpenXR Render Mode
            SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Android);

            // Fix all XR project validation issues
            // Fixing duplicate settings deletes the settings. So the issue will be ignored.
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android, new string[] { "The OpenXR Package Settings asset has duplicate settings and must be regenerated." });

            // ==== Extra settings for VIVE XR ==== 

            // ToDo 1
            // Enable Passthrough via script
            // https://developer.vive.com/resources/openxr/unity/tutorials/passthrough/
            // https://qiita.com/afjk/items/f723f6dd2101f9b85905

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
