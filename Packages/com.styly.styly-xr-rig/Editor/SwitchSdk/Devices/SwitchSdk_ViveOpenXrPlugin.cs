using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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

            // Set graphics APIs to OpenGLES3
            SetGraphicsAPIs(BuildTarget.Android,
                new List<GraphicsDeviceType> {
                    GraphicsDeviceType.OpenGLES3
                });

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

            // Post-switch SDK setup
            PostSwitchSdk();
        }
    }
}
