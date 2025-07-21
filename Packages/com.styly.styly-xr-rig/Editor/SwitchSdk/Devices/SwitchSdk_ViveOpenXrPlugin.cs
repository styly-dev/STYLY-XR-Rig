using UnityEditor;
using UnityEngine;
using static Styly.XRRig.SdkSwitcher.SwitchSdkUtils;

namespace Styly.XRRig.SdkSwitcher
{
    public partial class SwitchSdk
    {
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
    }
}
