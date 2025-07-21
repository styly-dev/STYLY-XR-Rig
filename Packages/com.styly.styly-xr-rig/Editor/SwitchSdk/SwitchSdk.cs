using UnityEditor;
using UnityEngine;
using static Styly.XRRig.SdkSwitcher.SwitchSdkUtils;

namespace Styly.XRRig.SdkSwitcher
{
    public partial class SwitchSdk
    {
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