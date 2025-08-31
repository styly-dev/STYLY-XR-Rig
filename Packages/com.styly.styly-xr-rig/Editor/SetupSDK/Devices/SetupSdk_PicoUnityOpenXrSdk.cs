using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;
#if USE_PICO
using Unity.XR.PXR;
using Unity.XR.OpenXR.Features.PICOSupport;
#endif

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdk_PicoUnityOpenXrSdk
    {
        private static readonly string packageIdentifier = "https://github.com/Pico-Developer/PICO-Unity-OpenXR-SDK.git#release_1.4.0";

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
                EnableXRFeatureSet(BuildTargetGroup.Android, "com.picoxr.openxr.features");

                EditorApplication.delayCall += Step3;
            }

            void Step3() // Enable OpenXR Features
            {
                EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
                {
                "com.unity.openxr.feature.input.handtracking",
                "com.pico.openxr.feature.passthrough"
                });

                EditorApplication.delayCall += Step4;
            }

            void Step4() // Enable Interaction Profiles
            {
                EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
                {
                "com.unity.openxr.feature.input.handinteraction",
                "com.unity.openxr.feature.input.PICO4touch",
                "com.unity.openxr.feature.input.PICO4Ultratouch"
                });

                EditorApplication.delayCall += Step5;
            }

            void Step5() // Setup Other Settings
            {
                // Set Android Minimum API Level
                SetAndroidMinimumApiLevel(AndroidSdkVersions.AndroidApiLevel26);

                // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
                ApplyStylyPipelineAsset();

                // Use the new input system only
                UseNewInputSystemOnly();

                // Set graphics APIs
                SetGraphicsAPIs(BuildTarget.Android,
                    new List<GraphicsDeviceType> {
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
                // ==== Extra settings for PICO XR ==== 

                // Set isCameraSubsystem to true
                SetFieldValueOfOpenXrFeature(BuildTargetGroup.Android, "com.pico.openxr.feature.passthrough", "isCameraSubsystem", true);

                // create PICOProjectSetting.asset if it does not exist
                CreatePicoProjectSettingAsset();

                // Configure PICO Hand Tracking
                SetupSdkUtils.ConfigurePicoHandTracking();

                // Create PXR_PlatformSetting.asset if it does not exist
                CreatePXRPlatformSettingAsset();
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

        private static void CreatePicoProjectSettingAsset()
        {
            // Create the PICO project setting asset if it does not exist
            // This is necessary for the PICO Hand Tracking feature to work properly
            var picoProjectSetting = Resources.Load("PICOProjectSetting");
#if USE_PICO
            if (picoProjectSetting == null)
            {
                PICOProjectSetting.GetProjectConfig();
            }
#endif
        }

        private static void CreatePXRPlatformSettingAsset()
        {
#if USE_PICO
            // Create PXR_PlatformSetting.asset if it does not exist
            var platformInstance = PXR_PlatformSetting.Instance;
            Debug.Log(platformInstance.appID);
            if (platformInstance.appID == null)
            {
                platformInstance.appID = "";
            }
#endif
        }
    }
}
