using UnityEditor;
using UnityEngine;
using static Styly.XRRig.SdkSwitcher.SwitchSdkUtils;

namespace Styly.XRRig.SdkSwitcher
{
    public class SwitchSdk
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

            // Extra settings for PICO XR
            // Set isCameraSubsystem to true
            SetFieldValueOfOpenXrFeature(BuildTargetGroup.Android, "com.pico.openxr.feature.passthrough", "isCameraSubsystem", true);

            // ToDo 2
            // PICO Supportの右の歯車ボタンを押しPICO Supportダイアログを開く
            // Feature Settingsから、 Hand Trackingにチェック
            // Hand Tracking SupportからControllers And Hands を選択
            // => デフォルトでこの設定になっている

        }

        public static void SwitchTo_ViveOpenXrPlugin()
        {
            // Add SDK Package
            AddUnityPackage("com.htc.upm.vive.openxr@2.5.1");

            // Enable the OpenXR Loader and set the XR Feature Set
            EnableXRPlugin(BuildTargetGroup.Android, typeof(UnityEngine.XR.OpenXR.OpenXRLoader));
            EnableXRFeatureSet(BuildTargetGroup.Android, "com.htc.vive.openxr.featureset.vivexr");

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

            // Extra settings for VIVE XR
            
            // ToDo 1
            // Graphics APIからVulkanを削除し、OpenGLES3のみにする

            // ToDo 2
            // Preferences > Graphics > Properties > Advanced PropertiesをAll Visibleにする

            // ToDo 3
            // HDR設定を変更する
            // Project Settings > Quality > MobileのRender Pipeline Assetを選択。
            // QualityのHDRをオフにする
            // または、QualityのHDRはONのまま、HDR Percisionを64bitに変更する

        }

        public static void SwitchTo_PolySpatialVisionOS()
        {
            Debug.LogError("PolySpatial VisionOS SDK is not implemented yet.");
        }

        public static void SwitchTo_MetaOpenXrSdk()
        {
            Debug.LogError("Meta OpenXR SDK is not implemented yet.");
        }

        public static void RemoveAllSdks()
        {
            // Remove all SDK packages from the project
            RemoveUnityPackage("com.unity.xr.openxr.picoxr");
            RemoveUnityPackage("com.htc.upm.vive.openxr");
        }

        public static void TestFunc()
        {
            // You can add more test logic here if needed
        }
    }
}