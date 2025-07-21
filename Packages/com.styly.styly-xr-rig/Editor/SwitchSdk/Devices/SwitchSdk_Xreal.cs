using UnityEditor;
using UnityEngine;
using static Styly.XRRig.SdkSwitcher.SwitchSdkUtils;

namespace Styly.XRRig.SdkSwitcher
{
    public partial class SwitchSdk
    {
        public static void SwitchTo_XrealSdk()
        {
            Debug.LogError("XREAL SDK is not implemented yet.");

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

            // Post-switch SDK setup
            PostSwitchSdk();
        }
    }
}