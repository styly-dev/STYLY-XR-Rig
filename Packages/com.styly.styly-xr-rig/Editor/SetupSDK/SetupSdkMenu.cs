using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdkMenu
    {
        [MenuItem("File/Switch HMD SDKs/Switch to PICO Unity OpenXR SDK")]
        static void SwitchTo_PICO()
        {
            SetupSdk_PicoUnityOpenXrSdk.InstallPackage();
        }

        [MenuItem("File/Switch HMD SDKs/Switch to VIVE OpenXR Plugin")]
        static void SwitchTo_VIVE()
        {
            SetupSdk_ViveOpenXrPlugin.InstallPackage();
        }

        [MenuItem("File/Switch HMD SDKs/Switch to Meta OpenXR SDK")]
        static void SwitchTo_Meta()
        {
            SetupSdk_MetaOpenXrSdk.InstallPackage();
        }

        [MenuItem("File/Switch HMD SDKs/Switch to PolySpatial (visionOS)")]
        static void SwitchTo_PolySpatial()
        {
            SetupSdk_PolySpatialVisionOS.InstallPackage();
        }

        [MenuItem("File/Switch HMD SDKs/Switch to XREAL SDK")]
        static void SwitchTo_Xreal()
        {
            SetupSdk_XrealSdk.InstallPackage();
        }

        [MenuItem("File/Switch HMD SDKs/Switch to Android XR")]
        static void SwitchTo_AndroidXR()
        {
            SetupSdk_AndroidXR.InstallPackage();
        }

        [MenuItem("File/Switch HMD SDKs/Switch to PC VR for Development")]
        static void SwitchTo_PcVrForDevelopment()
        {
            SetupSdk_PcVrForDevelopment.InstallPackage();
        }

        [MenuItem("File/Switch HMD SDKs/Remove all SDK packages")]
        static void RemoveAllSDKs()
        {
            // Remove all SDK packages from the project
            SetupSdk.RemoveAllSdks();

            // Set graphics APIs to Vulkan and OpenGLES3
            SetGraphicsAPIs(BuildTarget.Android,
                new List<GraphicsDeviceType> {
                    GraphicsDeviceType.Vulkan,
                    GraphicsDeviceType.OpenGLES3
                });
        }

        [MenuItem("File/Switch HMD SDKs/[Debug] Debug All Available identifiers for XR")]
        static void DebugAllAvailableInfoForAndroid()
        {
            SetupSdkUtils.DebugAllAvailableInfo(BuildTargetGroup.Android);
            SetupSdkUtils.DebugAllAvailableInfo(BuildTargetGroup.Standalone);
            SetupSdkUtils.DebugAllAvailableInfo(BuildTargetGroup.VisionOS);
        }

        [MenuItem("File/Switch HMD SDKs/Test Function")]
        static void TestFunc()
        {
            SetupSdk.TestFunc();
        }
    }
}