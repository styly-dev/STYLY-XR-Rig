using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdk_PolySpatialVisionOS
    {
        private static readonly string packageIdentifier = "com.unity.polyspatial.visionos@2.2.4";

        private static async void SetUpSdkSettings()
        {
            // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
            ApplyStylyPipelineAsset();

            // Use the new input system only
            UseNewInputSystemOnly();

            // Enable the OpenXR Loader and set the XR Feature Set
#if USE_POLYSPATIAL
            EnableXRPlugin(BuildTargetGroup.VisionOS, typeof(UnityEngine.XR.VisionOS.VisionOSLoader));
#endif

            // Wait for 1 frame to ensure the OpenXR Loader is initialized
            await WaitFramesAsync(1);

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.VisionOS);

            // ==== Extra settings for PolySpatial (visionOS) ====

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
