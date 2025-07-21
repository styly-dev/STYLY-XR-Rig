using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SdkSwitcher.SwitchSdkUtils;

namespace Styly.XRRig.SdkSwitcher
{
    public partial class SwitchSdk
    {
        public static void SwitchTo_PicoUnityOpenXrSdk()
        {
            // Add SDK Package
            AddUnityPackage("https://github.com/Pico-Developer/PICO-Unity-OpenXR-SDK.git#release_1.4.0");

            // Enable the OpenXR Loader and set the XR Feature Set
            EnableXRPlugin(BuildTargetGroup.Android, typeof(UnityEngine.XR.OpenXR.OpenXRLoader));
            EnableXRFeatureSet(BuildTargetGroup.Android, "com.picoxr.openxr.features");

            // Enable OpenXR Features
            EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
            {
                "com.unity.openxr.feature.input.handtracking",
                "com.pico.openxr.feature.passthrough"
            });

            // Enable Interaction Profiles
            EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
            {
                "com.unity.openxr.feature.input.handinteraction",
                "com.unity.openxr.feature.input.PICO4touch",
                "com.unity.openxr.feature.input.PICO4Ultratouch"
            });

            // Set OpenXR Render Mode to MultiPass
            SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Android);

            // Extra settings for PICO XR

            // Set graphics APIs to Vulkan and OpenGLES3
            SetGraphicsAPIs(BuildTarget.Android,
                new List<GraphicsDeviceType> {
                    GraphicsDeviceType.Vulkan,
                    GraphicsDeviceType.OpenGLES3
                });

            // Set isCameraSubsystem to true
            SetFieldValueOfOpenXrFeature(BuildTargetGroup.Android, "com.pico.openxr.feature.passthrough", "isCameraSubsystem", true);

            // ToDo 2
            // PICO Supportの右の歯車ボタンを押しPICO Supportダイアログを開く
            // Feature Settingsから、 Hand Trackingにチェック
            // Hand Tracking SupportからControllers And Hands を選択
            // 
            // => 上記のisCameraSubsystemとは違い、FeatureのFieldではない
            // データは Assets/Resources/PICOProjectSetting.asset に保存される
            // => 参考:
            // https://github.com/Pico-Developer/PICO-Unity-OpenXR-SDK/blob/3aa3e62bff41df618529eeb60ff02c29a515dafe/Editor/PICOFeatureEditor.cs#L43

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

            // Post-switch SDK setup
            PostSwitchSdk();

        }
    }
}
