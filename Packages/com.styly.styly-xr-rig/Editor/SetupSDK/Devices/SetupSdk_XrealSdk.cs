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

        private static async void SetUpSdkSettings()
        {
            // Set Android Minimum API Level
            SetAndroidMinimumApiLevel(AndroidSdkVersions.AndroidApiLevel29);
            
            // Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
            ApplyStylyPipelineAsset();

            // Use the new input system only
            UseNewInputSystemOnly();

            // Set graphics API
            SetGraphicsAPIs(BuildTarget.Android,
                new List<GraphicsDeviceType> {
                    GraphicsDeviceType.OpenGLES3
                });

            // Enable the OpenXR Loader and set the XR Feature Set
#if USE_XREAL
            EnableXRPlugin(BuildTargetGroup.Android, typeof(Unity.XR.XREAL.XREALXRLoader));
#endif
            // Wait for 2 frame to ensure the OpenXR Loader is initialized
            await WaitFramesAsync(2);

            // Enable OpenXR Features
            EnableOpenXrFeatures(BuildTargetGroup.Android, new string[]
            {
            });

            // Enable Interaction Profiles
            EnableInteractionProfiles(BuildTargetGroup.Android, new string[]
            {
            });

            // Set OpenXR Render Mode
            SetRenderMode(OpenXRSettings.RenderMode.SinglePassInstanced, BuildTargetGroup.Android);

            // Fix all XR project validation issues
            XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android);

            // ==== Extra settings for XREAL ====
#if USE_XREAL
            SetInitialInputSourceToHands();
#endif
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

#if USE_XREAL
        private static void SetInitialInputSourceToHands()
        {
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
        }
#endif
    }
}
