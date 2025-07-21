using UnityEditor;
using UnityEngine;
using static Styly.XRRig.SdkSwitcher.SwitchSdkUtils;

namespace Styly.XRRig.SdkSwitcher
{
    public partial class SwitchSdk
    {
        public static void SwitchTo_PolySpatialVisionOS()
        {
            Debug.LogError("PolySpatial VisionOS SDK is not implemented yet.");

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.VisionOS);

            // Post-switch SDK setup
            PostSwitchSdk();
        }
    }
}
