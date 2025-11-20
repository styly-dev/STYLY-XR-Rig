using System.Collections.Generic;
#if USE_XREAL
using Unity.XR.XREAL;
#endif
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using static Styly.XRRig.SetupSdk.SetupSdkUtils;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdk_XrealSdk
    {
        private static readonly string packageIdentifier = "https://public-resource.xreal.com/download/XREALSDK_Release_3.0.0.20250314/com.xreal.xr.tar.gz";

        private static void SetUpSdkSettings()
        {
            EditorApplication.delayCall += Step1;

            void Step1()  // Enable the OpenXR Loader
            {
#if USE_XREAL
            EnableXRPlugin(BuildTargetGroup.Android, typeof(Unity.XR.XREAL.XREALXRLoader).ToString());
#endif
                EditorApplication.delayCall += Step2;
            }

            void Step2() // Enable the XR Feature Set
            {
                EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
                {
                });

                EditorApplication.delayCall += Step3;
            }

            void Step3() // Enable OpenXR Features
            {
                // Nothing to do
                EditorApplication.delayCall += Step4;
            }

            void Step4() // Enable Interaction Profiles
            {
                EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
                {
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

                // Set graphics APIs
                SetGraphicsAPIs(BuildTarget.Android,
                    new List<GraphicsDeviceType> {
                    GraphicsDeviceType.OpenGLES3
                    });

                // Set OpenXR Render Mode
                SetRenderMode(OpenXRSettings.RenderMode.SinglePassInstanced, BuildTargetGroup.Android);

                EditorApplication.delayCall += Step6;
            }

            void Step6() // Fix XR Project Validation Issues
            {
                XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

                EditorApplication.delayCall += Step7;
            }

            void Step7() // Additional Settings
            {
                // ==== Extra settings for XREAL ====
                SetInitialInputSourceToHands();
            }
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

        private static void SetInitialInputSourceToHands()
        {
#if USE_XREAL
            // find the XREALSettings asset in the project
            XREALSettings xrealSettings = FindXREALSettingsAsset();
        
            if (xrealSettings == null)
            {
                Debug.LogError("XREALSettings asset not found in the project");
                return;
            }
        
            // Create a SerializedObject to modify the InitialInputSource property
            SerializedObject serializedObject = new SerializedObject(xrealSettings);
            SerializedProperty initialInputSourceProperty = serializedObject.FindProperty("InitialInputSource");
        
            if (initialInputSourceProperty != null)
            {
                Debug.Log($"Current InitialInputSource value: {initialInputSourceProperty.intValue}");

                // Set the InitialInputSource to Hands (value: 2)
                initialInputSourceProperty.intValue = 2;
            
                serializedObject.ApplyModifiedProperties();
            
                EditorUtility.SetDirty(xrealSettings);
                AssetDatabase.SaveAssets();
            
                Debug.Log("InitialInputSource has been set to Hands (value: 2)");
            }
            else
            {
                Debug.LogError("InitialInputSource property not found in XREALSettings");
            }
        }
        
        /// <summary>
        /// Finds the XREALSettings asset in the project.
        /// </summary>
        private static XREALSettings FindXREALSettingsAsset()
        {
            // AssetDatabase.FindAssets returns an array of GUIDs for assets that match the specified type
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(XREALSettings).Name}");
        
            if (guids.Length == 0)
            {
                Debug.LogWarning("No XREALSettings assets found in the project");
                return null;
            }
        
            if (guids.Length > 1)
            {
                Debug.LogWarning($"Multiple XREALSettings assets found in the project ({guids.Length}). Using the first one.");
            }
        
            // Load the first found XREALSettings asset using its GUID
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            XREALSettings settings = AssetDatabase.LoadAssetAtPath<XREALSettings>(assetPath);
        
            Debug.Log($"Found XREALSettings asset at: {assetPath}");
            return settings;
#endif
        }
    }
}
