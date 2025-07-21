using UnityEditor;
using UnityEngine;

namespace Styly.XRRig.SdkSwitcher
{
    public class SwitchSdkMenu
    {
        [MenuItem("File/Switch HMD SDKs/Switch to PICO Unity OpenXR SDK")]
        static void SwitchTo_PICO()
        {
            SwitchSdk.SwitchTo_PicoUnityOpenXrSdk();
        }

        [MenuItem("File/Switch HMD SDKs/Switch to VIVE OpenXR Plugin")]
        static void SwitchTo_VIVE()
        {
            SwitchSdk.SwitchTo_ViveOpenXrPlugin();
        }

        [MenuItem("File/Switch HMD SDKs/Switch to Meta OpenXR SDK")]
        static void SwitchTo_Meta()
        {
            SwitchSdk.SwitchTo_MetaOpenXrSdk();
        }

        [MenuItem("File/Switch HMD SDKs/Switch to PolySpatial (visionOS)")]
        static void SwitchTo_PolySpatial()
        {
            SwitchSdk.SwitchTo_PolySpatialVisionOS();
        }

        [MenuItem("File/Switch HMD SDKs/Switch to XREAL SDK")]
        static void SwitchTo_Xreal()
        {
            SwitchSdk.SwitchTo_XrealSdk();
        }

        [MenuItem("File/Switch HMD SDKs/Remove all SDK packages")]
        static void RemoveAllSDKs()
        {
            SwitchSdk.RemoveAllSdks();
        }

        [MenuItem("File/Switch HMD SDKs/[Debug] Debug All Available Info for Android")]
        static void DebugAllAvailableInfoForAndroid()
        {
            SwitchSdkUtils.DebugAllAvailableInfo(BuildTargetGroup.Android);
        }

        [MenuItem("File/Switch HMD SDKs/Test Function")]
        static void TestFunc()
        {
            SwitchSdk.TestFunc();
        }
    }
}