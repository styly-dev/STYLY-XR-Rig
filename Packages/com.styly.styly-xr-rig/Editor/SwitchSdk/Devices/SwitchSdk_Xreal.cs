using UnityEditor;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SdkSwitcher.SwitchSdkUtils;

namespace Styly.XRRig.SdkSwitcher
{
    public partial class SwitchSdk
    {
        public static void SwitchTo_XrealSdk()
        {
            Debug.LogError("XREAL SDK is not implemented yet.");

            // Set OpenXR Render Mode to MultiPass
            SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Android);

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

            // Post-switch SDK setup
            PostSwitchSdk();
        }
    }
}