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
        // Setup code for VisionOS is different from code for other SDKs.
        // InstallPackageAndAssignLoaderForBuildTarget cannot take SDK version.
        // Set com.unity.xr.visionos instead of com.unity.polyspatial.visionos
        private static readonly string packageIdentifier = "com.unity.xr.visionos"; 

        private static void SetUpSdkSettings()
        {
            EditorApplication.delayCall += Step1;

            void Step1()  // Enable Loader
            {
                // Nothing to do

                EditorApplication.delayCall += Step2;
            }

            void Step2() // Enable the XR Feature Set
            {
                // Nothing to do

                EditorApplication.delayCall += Step3;
            }

            void Step3() // Enable OpenXR Features
            {
                // Nothing to do

                EditorApplication.delayCall += Step4;
            }

            void Step4() // Enable Interaction Profiles
            {
                // Nothing to do

                EditorApplication.delayCall += Step5;
            }

            void Step5() // Setup Other Settings
            {
                // Set Android Minimum API Level
                // Nothing to do

                // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
                ApplyStylyPipelineAsset();

                // Use the new input system only
                UseNewInputSystemOnly();

                // Set graphics APIs
                // Nothing to do

                // Set OpenXR Render Mode
                // Nothing to do

                EditorApplication.delayCall += Step6;
            }

            void Step6() // Fix XR Project Validation Issues
            {
                XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.VisionOS);

                EditorApplication.delayCall += Step7;
            }

            void Step7() // Additional Settings
            {
                // Nothing to do
            }
        }

        #region CodeForVisionOS
        public static async void InstallPackage()
        {
            // Install package and assign loader
            if (InstallPackageAndAssignLoaderForBuildTarget("com.unity.xr.visionos", "UnityEngine.XR.VisionOS.VisionOSLoader", BuildTargetGroup.VisionOS)) { SessionState.SetBool(packageIdentifier, true); }
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
