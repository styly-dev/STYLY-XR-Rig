using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public partial class SetupSdk
    {
        public static void RemoveAllSdks()
        {
            // Remove all SDK packages from the project
            RemoveUnityPackage("com.unity.xr.openxr.picoxr");
            RemoveUnityPackage("com.htc.upm.vive.openxr");

            // Set default graphics APIs
            SetGraphicsAPIs(BuildTarget.Android,
            new List<GraphicsDeviceType> {
                GraphicsDeviceType.Vulkan,
                GraphicsDeviceType.OpenGLES3
            });
        }

        public static void TestFunc()
        {
            // You can add more test logic here if needed
            string packageIdentifier = "com.htc.upm.vive.openxr@2.5.1";
            AddUnityPackage(packageIdentifier);

        }
    }
}