using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdk_AndroidXR
    {
        private static readonly string packageIdentifier = "com.unity.xr.androidxr-openxr@1.4.1";

        private static void SetUpSdkSettings()
        {

            EditorApplication.delayCall += Step1;

            void Step1()  // Enable the OpenXR Loader
            {
                EnableXRPlugin(BuildTargetGroup.Android, "UnityEngine.XR.OpenXR.OpenXRLoader");

                EditorApplication.delayCall += Step2;
            }

            void Step2() // Enable the XR Feature Set
            {
                EnableXRFeatureSet(BuildTargetGroup.Android, "com.unity.openxr.featureset.android");

                EditorApplication.delayCall += Step3;
            }

            void Step3() // Enable OpenXR Features
            {
                EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
                {
                "com.unity.openxr.feature.androidxr-support",
                "com.unity.openxr.feature.input.handtracking",
                "com.unity.openxr.feature.arfoundation-androidxr-anchor",
                "com.unity.openxr.feature.arfoundation-androidxr-camera",
                "com.unity.openxr.feature.arfoundation-androidxr-face",
                "com.unity.openxr.feature.arfoundation-androidxr-occlusion",
                "com.unity.openxr.feature.arfoundation-androidxr-plane",
                "com.unity.openxr.feature.arfoundation-androidxr-raycast",
                "com.unity.openxr.feature.arfoundation-androidxr-session",
                "com.unity.openxr.feature.androidxr-display-utilities",
                "com.unity.openxr.feature.androidxr-hand-mesh-data"
                });

                EditorApplication.delayCall += Step4;
            }

            void Step4() // Enable Interaction Profiles
            {
                EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
                {
                "com.unity.openxr.feature.input.handinteraction",
                "com.unity.openxr.feature.input.oculustouch",
                "com.unity.openxr.feature.input.khrsimpleprofile"
                });

                EditorApplication.delayCall += Step5;
            }

            void Step5() // Setup Other Settings
            {
                // Set Android Minimum API Level
                SetAndroidMinimumApiLevel(AndroidSdkVersions.AndroidApiLevel29);

                // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
                ApplyStylyPipelineAsset();

                // Use the new input system only
                UseNewInputSystemOnly();

                // Set graphics APIs to Vulkan and OpenGLES3
                SetGraphicsAPIs(BuildTarget.Android,
                    new List<GraphicsDeviceType> {
                    GraphicsDeviceType.Vulkan
                    });

                // Set OpenXR Render Mode
                SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Android);

                EditorApplication.delayCall += Step6;
            }

            void Step6() // Fix XR Project Validation Issues
            {
                XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

                EditorApplication.delayCall += Step7;
            }

            void Step7() // Additional Settings
            {
                UpgradeControllerMaterials();
            }

            // ==== Extra settings for Android XR ====

            // Note: AR Camera Manager / AR Camera Background are added at runtime by
            // Styly.XRRig.PassthroughManager.SetUpCameraForPassthrough() for all devices,
            // so Passthrough works without extra setup here.
            // https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.4/manual/features/camera.html

            // ToDo
            // For functionalities not provided by the above package, Google offers additional extensions.
            // https://github.com/android/android-xr-unity-package

        }

        // XRI sample controllers ship with Built-in Standard materials. Convert only
        // those imported sample assets when selecting the rig's URP configuration.
        internal static int UpgradeControllerMaterials()
        {
            int upgraded = 0;
            foreach (string guid in AssetDatabase.FindAssets("Controller_ t:Material"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/Samples/XR Interaction Toolkit/", System.StringComparison.Ordinal)
                    || !path.Contains("/Starter Assets/Materials/")) { continue; }

                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null || material.shader == null || material.shader.name != "Standard") { continue; }
                if (material.name != "Controller_Grey" && material.name != "Controller_White") { continue; }

                new UnityEditor.Rendering.Universal.StandardUpgrader("Standard").Upgrade(
                    material, UnityEditor.Rendering.MaterialUpgrader.UpgradeFlags.None);
                EditorUtility.SetDirty(material);
                upgraded++;
            }
            if (upgraded > 0) { AssetDatabase.SaveAssets(); }
            return upgraded;
        }

        #region CommonCode
        public static async void InstallPackage()
        {
            if (AddUnityPackage(packageIdentifier)) { SessionState.SetBool(packageIdentifier, true); }
            await WaitFramesAsync(1);
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (!SessionState.GetBool(packageIdentifier, false)) { return; }
            SessionState.EraseBool(packageIdentifier);
            SetUpSdkSettings();
        }
        #endregion
    }
}
