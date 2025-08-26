using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdkMenu
    {
        [MenuItem("File/Setup HMD SDKs/Setup PICO Unity OpenXR SDK")]
        static void SwitchTo_PICO()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            SetupSdk_PicoUnityOpenXrSdk.InstallPackage();
        }

        [MenuItem("File/Setup HMD SDKs/Setup VIVE OpenXR Plugin")]
        static void SwitchTo_VIVE()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            SetupSdk_ViveOpenXrPlugin.InstallPackage();
        }

        [MenuItem("File/Setup HMD SDKs/Setup Meta OpenXR SDK")]
        static void SwitchTo_Meta()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            SetupSdk_MetaOpenXrSdk.InstallPackage();
        }

        [MenuItem("File/Setup HMD SDKs/Setup PolySpatial (visionOS)")]
        static void SwitchTo_PolySpatial()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.VisionOS, BuildTarget.VisionOS);
            SetupSdk_PolySpatialVisionOS.InstallPackage();
        }

        [MenuItem("File/Setup HMD SDKs/Setup XREAL SDK")]
        static void SwitchTo_Xreal()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            SetupSdk_XrealSdk.InstallPackage();
        }

        [MenuItem("File/Setup HMD SDKs/Setup Android XR")]
        static void SwitchTo_AndroidXR()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            SetupSdk_AndroidXR.InstallPackage();
        }

        [MenuItem("File/Setup HMD SDKs/Setup PC VR for Development")]
        static void SwitchTo_PcVrForDevelopment()
        {
            SetupSdk_PcVrForDevelopment.InstallPackage();
        }

        [MenuItem("File/Setup HMD SDKs/Remove all SDK packages")]
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

        [MenuItem("File/Setup HMD SDKs/[Debug] Debug All Available identifiers for XR")]
        static void DebugAllAvailableInfoForAndroid()
        {
            SetupSdkUtils.DebugAllAvailableInfo(BuildTargetGroup.Android);
            SetupSdkUtils.DebugAllAvailableInfo(BuildTargetGroup.Standalone);
            SetupSdkUtils.DebugAllAvailableInfo(BuildTargetGroup.VisionOS);
        }

        [MenuItem("File/Setup HMD SDKs/Test Function")]
        static void TestFunc()
        {
            SetupSdk.TestFunc();
        }
    }
}