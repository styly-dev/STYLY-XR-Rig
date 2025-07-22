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
        public static void SwitchPC()
        {
            // ApplyStylyPipelineAsset();
            // UseNewInputSystemOnly();

            // Set OpenXR Render Mode to MultiPass for PC
            SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Standalone);
        }

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

            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

            // ApplyStylyPipelineAsset();

            // UseNewInputSystemOnly();

            // SetGraphicsAPIs(BuildTarget.Android,
            //     new List<GraphicsDeviceType> {
            //         GraphicsDeviceType.Vulkan,
            //         GraphicsDeviceType.OpenGLES3
            //     });
        }
    }
}