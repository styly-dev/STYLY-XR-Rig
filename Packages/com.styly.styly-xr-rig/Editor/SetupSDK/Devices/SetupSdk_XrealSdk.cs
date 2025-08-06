using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdk_XrealSdk
    {
        private static readonly string packageIdentifier = "https://public-resource.xreal.com/download/XREALSDK_Release_3.0.0.20250314/com.xreal.xr.tar.gz";

        private static async void SetUpSdkSettings()
        {
            // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
            ApplyStylyPipelineAsset();

            // Use the new input system only
            UseNewInputSystemOnly();

            // Set graphics API
            SetGraphicsAPIs(BuildTarget.Android,
                new List<GraphicsDeviceType> {
                    GraphicsDeviceType.OpenGLES3
                });

            // Enable the OpenXR Loader and set the XR Feature Set
#if USE_XREAL
            EnableXRPlugin(BuildTargetGroup.Android, typeof(Unity.XR.XREAL.XREALXRLoader));
#endif
            // Wait for 1 frame to ensure the OpenXR Loader is initialized
            await WaitFramesAsync(1);

            // Enable OpenXR Features
            EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
            {
            });

            // Enable Interaction Profiles
            EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
            {
            });

            // Set OpenXR Render Mode
            SetRenderMode(OpenXRSettings.RenderMode.SinglePassInstanced, BuildTargetGroup.Android);

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

            // ==== Extra settings for XREAL ====


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
