using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.Rendering;
using UnityEditor.Rendering;
using System.Threading.Tasks;
using UnityEditor.Compilation;

namespace Styly.XRRig.SetupSdk
{
    public class SetupSdkUtils
    {
        /// <summary>
        /// The path to the STYLY Mobile Render Pipeline Asset. 
        /// </summary>
        private static readonly string STYLY_Mobile_RPAsset_path = "Packages/com.styly.styly-xr-rig/Runtime/Settings/STYLY_Mobile_RPAsset.asset";

        /// <summary>
        /// Waits for a specified number of frames to pass.
        /// </summary>
        public static Task WaitFramesAsync(int frames)
        {
            if (frames <= 0) return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();
            int remaining = frames;

            void Tick()
            {
                if (--remaining == 0)
                {
                    EditorApplication.update -= Tick;
                    tcs.TrySetResult(true);
                }
            }

            EditorApplication.update += Tick;
            return tcs.Task;
        }

        /// <summary>
        /// Sets Android Minimum API Level using AndroidSdkVersions enum.
        /// </summary>
        public static void SetAndroidMinimumApiLevel(AndroidSdkVersions version)
        {
            if (PlayerSettings.Android.minSdkVersion == version) return;

            PlayerSettings.Android.minSdkVersion = version;
            Debug.Log($"Set Android Minimum API Level to {(int)version} ({version}).");
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Sets the OpenXR Render Mode for the specified build target group.
        /// This will change the render mode to either "Single Pass" or "Multi Pass" depending on the mode specified.
        /// </summary>
        /// <param name="mode">The render mode to set (e.g. OpenXRSettings.RenderMode.SinglePass or OpenXRSettings.RenderMode.MultiPass).</param>
        /// <param name="group">The build target group for which to set the render mode.</param>
        /// <example>
        /// SetRenderMode(OpenXRSettings.RenderMode.MultiPass, BuildTargetGroup.Android);
        /// </example>
        public static void SetRenderMode(OpenXRSettings.RenderMode mode, BuildTargetGroup group)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if (settings == null)
            {
                Debug.LogWarning($"OpenXRSettings for {group} not found.");
                return;
            }

            if (settings.renderMode != mode)
            {
                Undo.RecordObject(settings, $"Set OpenXR Render Mode ({group})");
                settings.renderMode = mode;
                EditorUtility.SetDirty(settings);
                Debug.Log($"Set OpenXR Render Mode for {group} to {mode}");
            }
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Sets the active input handling to "Input System Package (New)".
        /// This will ensure that the new input system is used exclusively.
        /// Note: This change will take effect after restarting the Unity Editor.
        /// </summary>
        public static void UseNewInputSystemOnly()
        {
            // Get the PlayerSettings asset
            var playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault();
            if (playerSettings == null)
            {
                Debug.LogError("PlayerSettings asset not found.");
                return;
            }

            var so = new SerializedObject(playerSettings);

            // enum: 0=Old, 1=New, 2=Both
            const string kActiveInputHandler = "activeInputHandler";
            var prop = so.FindProperty(kActiveInputHandler);

            if (prop == null)
            {
                Debug.LogError($"Property '{kActiveInputHandler}' not found.");
                return;
            }

            if (prop.intValue != 1)
            {
                prop.intValue = 1; // 1 = Input System Package (New)
                so.ApplyModifiedProperties();
                Debug.Log("Active Input Handling set to 'Input System Package (New)'. Changes will take effect after restarting the Unity Editor.");
            }
        }

        /// <summary>
        /// Applies the STYLY Mobile Render Pipeline Asset to the GraphicsSettings and QualitySettings.
        /// </summary>
        public static void ApplyStylyPipelineAsset()
        {
            // Get the STYLY Mobile Render Pipeline Asset
            var rpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(STYLY_Mobile_RPAsset_path);
            if (rpAsset == null)
            {
                Debug.LogError($"Failed to load STYLY Mobile Render Pipeline Asset at {STYLY_Mobile_RPAsset_path}");
                return;
            }

            // Apply the Render Pipeline Asset to GraphicsSettings and QualitySettings
            ApplyPipelineAsset(rpAsset);
        }

        /// <summary>
        /// Updates both GraphicsSettings and QualitySettings with the specified RenderPipelineAsset.
        /// </summary>
        public static void ApplyPipelineAsset(RenderPipelineAsset asset)
        {
            // 1) Set for the entire project
            GraphicsSettings.defaultRenderPipeline = asset;

            // 2) Set for all Quality levels
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = asset;
            }

            // 3) Persist changes
            EditorUtility.SetDirty(GraphicsSettings.GetGraphicsSettings());
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Sets the graphics APIs for a specific build target.
        /// </summary>
        /// <param name="target">The build target for which to set the graphics APIs.</param>
        /// <param name="graphicsDeviceTypes">A list of graphics device types to set for the target.</param>
        /// <example>
        /// SetGraphicsAPIs(BuildTarget.Android, new List<GraphicsDeviceType> { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });
        /// </example>
        public static void SetGraphicsAPIs(BuildTarget target, List<GraphicsDeviceType> graphicsDeviceTypes)
        {
            // Turn off automatic graphics API selection and switch to manual list
            PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);

            // The order of the list determines priority from left to right
            PlayerSettings.SetGraphicsAPIs(target, graphicsDeviceTypes.ToArray());
        }

        /// <summary>
        /// Debugs all available XR settings information for the specified build target group.
        /// This includes XR Loaders, XR Feature Sets, OpenXR Features, and Interaction Profiles.
        /// The output is logged to the Unity console.
        /// <param name="buildTargetGroup">The build target group for which to debug XR settings.</param>
        public static void DebugAllAvailableInfo(BuildTargetGroup buildTargetGroup)
        {
            List<string> DebugLines = new();
            DebugLines.Add($"=== Debugging XR Settings for {buildTargetGroup} ===");
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);

            // All XR Loaders
            DebugLines.Add("\n=== XR Loaders ===");
            var pkgs = UnityEditor.XR.Management.Metadata.XRPackageMetadataStore.GetAllPackageMetadata();
            foreach (var pkg in pkgs)
            {
                foreach (UnityEditor.XR.Management.Metadata.IXRLoaderMetadata meta in pkg.metadata.loaderMetadata)
                {
                    if (meta.supportedBuildTargets.Contains(buildTargetGroup))
                    {
                        DebugLines.Add($"Loader: {meta.loaderName}, Type: {meta.loaderType}");
                    }
                }
            }

            // All XR Feature Sets
            DebugLines.Add("\n=== All XR Feature Sets ===");
            foreach (var set in OpenXRFeatureSetManager.FeatureSetsForBuildTarget(buildTargetGroup))
            {
                DebugLines.Add($"Feature Set: {set.featureSetId}, Enabled: {set.isEnabled}, Installed: {set.isInstalled}");
            }

            // All OpenXR Features
            DebugLines.Add("\n=== All OpenXR Features ===");
            foreach (var feature in settings.GetFeatures())
            {
                FieldInfo fieldInfo = feature.GetType().GetField("featureIdInternal", BindingFlags.NonPublic | BindingFlags.Instance);
                string featureIdInternal = (string)fieldInfo.GetValue(feature);
                DebugLines.Add($"OpenXR Feature: {feature.name}, ID: {featureIdInternal}, Type: {feature.GetType().Name}, Enabled: {feature.enabled}");
            }

            // All Interaction Profiles
            DebugLines.Add("\n=== All Interaction Profiles ===");
            foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
            {
                FieldInfo fieldInfo = feature.GetType().GetField("featureIdInternal", BindingFlags.NonPublic | BindingFlags.Instance);
                string featureIdInternal = (string)fieldInfo.GetValue(feature);
                DebugLines.Add($"Interaction Profile Feature: {feature.name}, ID: {featureIdInternal}, Type: {feature.GetType().Name}, Enabled: {feature.enabled}");
            }

            // Add newlines for better readability
            DebugLines.Add("\n\n");

            // Output all debug lines in the console at one time
            Debug.Log(string.Join("\n", DebugLines));
        }

        /// <summary>
        /// Clears all interaction profiles for the specified build target group.
        /// This will disable all interaction profiles, effectively resetting the OpenXR interaction settings for that build
        /// target group.
        /// </summary>
        /// <param name="buildTargetGroup">The build target group for which to clear interaction profiles.</param>
        public static void ClearAllInteractionProfiles(BuildTargetGroup buildTargetGroup)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
            foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
            {
                feature.enabled = false;
            }
        }

        /// <summary>
        /// Enables a list of OpenXR interaction profiles by their internal IDs for the given build target group.
        /// Data will be saved to `Assets/XR/Settings/OpenXR Package Settings.asset`
        /// </summary>
        public static void EnableInteractionProfiles(BuildTargetGroup buildTargetGroup, string[] featureIdInternal)
        {
            ClearAllInteractionProfiles(buildTargetGroup);
            foreach (var id in featureIdInternal)
            {
                EnableInteractionProfile(buildTargetGroup, id);
            }
        }

        /// <summary>
        /// Enables a specific OpenXR interaction profile by its internal ID for the given build target group
        /// Data will be saved to `Assets/XR/Settings/OpenXR Package Settings.asset`
        /// </summary>
        /// <param name="buildTargetGroup">The build target group for which to enable the interaction profile.</param>
        /// <param name="featureIdInternal">The internal ID of the OpenXR interaction profile to enable (e.g. "com.unity.openxr.feature.input.PICO4touch").</param>
        public static void EnableInteractionProfile(BuildTargetGroup buildTargetGroup, string featureIdInternal)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
            foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
            {
                FieldInfo fieldInfo = feature.GetType().GetField("featureIdInternal", BindingFlags.NonPublic | BindingFlags.Instance);
                string tmp_featureIdInternal = (string)fieldInfo.GetValue(feature);

                if (tmp_featureIdInternal == featureIdInternal)
                {
                    // Enable the feature
                    feature.enabled = true;
                    Debug.Log($"Enabled OpenXR Interaction feature: {feature.name}");
                }
            }
        }

        /// <summary>
        /// Clears all OpenXR features for the specified build target group.
        /// This will disable all features, effectively resetting the OpenXR settings for that build target group
        /// </summary>
        public static void ClearAllOpenXrFeatures(BuildTargetGroup buildTargetGroup)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
            foreach (var feature in settings.GetFeatures())
            {
                feature.enabled = false;
            }
        }

        /// <summary>
        /// Enables a list of OpenXR features by their internal IDs for the given build target group
        /// Data will be saved to `Assets/XR/Settings/OpenXR Package Settings.asset`
        /// </summary>
        /// <param name="buildTargetGroup">The build target group for which to enable the features.</param>
        /// <param name="featureIdsInternal">A list of internal IDs of the OpenXR features to enable (e.g. "com.pico.openxr.feature.passthrough").</param>
        /// <example>
        /// EnableOpenXrFeatures(BuildTargetGroup.Android, new List<string> { "com.pico.openxr.feature.passthrough", "com.pico.openxr.feature.foveation" });
        /// </example>
        public static void EnableOpenXrFeatures(BuildTargetGroup buildTargetGroup, string[] featureIdsInternal)
        {
            ClearAllOpenXrFeatures(buildTargetGroup);
            foreach (var id in featureIdsInternal)
            {
                EnableOpenXrFeature(buildTargetGroup, id);
            }
        }

        /// <summary>
        /// Enables a specific OpenXR feature by its internal ID for the given build target group.
        /// Data will be saved to `Assets/XR/Settings/OpenXR Package Settings.asset`
        /// </summary>
        /// <param name="buildTargetGroup">The build target group for which to enable the feature.</param>
        /// <param name="featureIdInternal">The internal ID of the OpenXR feature to enable (e.g. "com.pico.openxr.feature.passthrough").</param>
        public static void EnableOpenXrFeature(BuildTargetGroup buildTargetGroup, string featureIdInternal)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
            foreach (var feature in settings.GetFeatures())
            {
                FieldInfo fieldInfo = feature.GetType().GetField("featureIdInternal", BindingFlags.NonPublic | BindingFlags.Instance);
                string tmp_featureIdInternal = (string)fieldInfo.GetValue(feature);

                if (tmp_featureIdInternal == featureIdInternal)
                {
                    // Enable the feature
                    feature.enabled = true;
                    Debug.Log($"Enabled OpenXR feature: {feature.name}");
                }
            }
        }

        /// <summary>
        /// Sets the value of a specific field in an OpenXR feature by its internal ID.
        /// </summary>
        /// /// <param name="buildTargetGroup">The build target group for which to set the field value.</param>
        /// <param name="featureIdInternal">The internal ID of the OpenXR feature (e.g. "com.pico.openxr.feature.passthrough").</param>
        /// <param name="fieldName">The name of the field to set (e.g. "isCameraSubsystem").</param>
        /// <param name="value">The value to set for the field.</param> 
        public static void SetFieldValueOfOpenXrFeature(BuildTargetGroup buildTargetGroup, string featureIdInternal, string fieldName, object value)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
            foreach (var feature in settings.GetFeatures())
            {
                FieldInfo fieldInfo = feature.GetType().GetField("featureIdInternal", BindingFlags.NonPublic | BindingFlags.Instance);
                string tmp_featureIdInternal = (string)fieldInfo.GetValue(feature);

                if (tmp_featureIdInternal == featureIdInternal)
                {
                    var type = feature.GetType();
                    var field = type.GetField(fieldName);
                    if (field != null)
                    {
                        field.SetValue(feature, value);
                        Debug.Log($"Set {fieldName} of {feature.GetType().Name} to {value}");
                    }
                    else
                    {
                        Debug.LogError($"Field '{fieldName}' not found in {feature.GetType().Name}");
                    }
                }
                // Mark the feature as dirty to ensure changes are saved
                EditorUtility.SetDirty(feature);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Enables a specific XR plugin for the given build target group.
        /// /// Data will be saved to `Assets/XR/XRGeneralSettingsPerBuildTarget.asset`
        /// </summary>
        /// <param name="buildTargetGroup">The build target group for which to enable the plugin.</param>
        /// <param name="loaderType">The type of the XRLoader to enable (e.g. UnityEngine.XR.OpenXR.OpenXRLoader).</param>
        /// <example>
        /// EnableXRPlugin(BuildTargetGroup.Standalone, typeof(UnityEngine.XR.OpenXR.OpenXRLoader));
        /// </example>
        public static void EnableXRPlugin(BuildTargetGroup buildTargetGroup, Type loaderType)
        {
            Debug.Log($"Enabling {loaderType.Name} for {buildTargetGroup}...");

            // Get the current XRGeneralSettings instance
            XRGeneralSettings xrGeneralSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);

            if (xrGeneralSettings == null)
            {
                Debug.LogError("XRGeneralSettings is null. Make sure XR Plugin Management is installed and configured.");
                return;
            }

            // Get or create the XRManagerSettings instance
            XRManagerSettings xrManagerSettings = xrGeneralSettings.AssignedSettings;
            if (xrManagerSettings == null)
            {
                xrManagerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
                xrGeneralSettings.AssignedSettings = xrManagerSettings;
            }

            // Loop through existing loaders and disable them if they are not the one we want to enable
            foreach (var loader in xrManagerSettings.activeLoaders.ToList())
            {
                if (loader.GetType() != loaderType)
                {
                    xrManagerSettings.TryRemoveLoader(loader);
                    Debug.Log($"Disabling loader: {loader.GetType().Name}");
                }
            }

            // Enable the specified loader if it's not already enabled
            if (!xrManagerSettings.activeLoaders.Any(loader => loader.GetType() == loaderType))
            {
                var xrLoader = ScriptableObject.CreateInstance(loaderType) as XRLoader;
                xrManagerSettings.TryAddLoader(xrLoader);
                
                var loaderAssetPath = $"Assets/XR/Loaders/{loaderType.Name}.asset";
                AssetDatabase.CreateAsset(xrLoader, loaderAssetPath);
                xrManagerSettings.TryAddLoader(xrLoader);
                EditorUtility.SetDirty(xrLoader);
                AssetDatabase.SaveAssets();
            }

            // Set the initialization mode to OnDemand or Automatic as needed
            xrManagerSettings.automaticLoading = true;
            xrManagerSettings.automaticRunning = true;

            // Save the changes
            EditorUtility.SetDirty(xrGeneralSettings);
            EditorUtility.SetDirty(xrManagerSettings);
            AssetDatabase.SaveAssets();

            Debug.Log($"{loaderType.Name} has been enabled successfully for {buildTargetGroup}.");
        }

        /// <summary>
        /// Enables a specific OpenXR feature set for the given build target group.
        /// Applies in two phases across multiple groups and frames to avoid custom runtime loader conflicts.
        /// </summary>
        public static void EnableXRFeatureSet(BuildTargetGroup buildTargetGroup, string featureSetId)
        {
            if (s_IsSwitchingFeatureSet) { Debug.LogWarning("Feature set switch already in progress."); return; }
            s_IsSwitchingFeatureSet = true;

            EditorApplication.delayCall += async () =>
            {
                try
                {
                    // Resolve target feature set upfront.
                    var targetSets = OpenXRFeatureSetManager.FeatureSetsForBuildTarget(buildTargetGroup).ToList();
                    var targetSet = targetSets.FirstOrDefault(set => set.featureSetId == featureSetId);
                    if (targetSet == null)
                    {
                        Debug.LogError($"Feature set with ID '{featureSetId}' not found for build target group '{buildTargetGroup}'.");
                        return;
                    }
                    if (!targetSet.isInstalled)
                    {
                        Debug.LogError($"Feature set with ID '{featureSetId}' is not installed.");
                        return;
                    }

                    // Phase 1: Disable all sets and all features for relevant groups, apply, save.
                    foreach (var group in s_RelevantGroups)
                    {
                        var sets = OpenXRFeatureSetManager.FeatureSetsForBuildTarget(group).ToList();
                        foreach (var set in sets)
                        {
                            if (set.isEnabled)
                            {
                                set.isEnabled = false;
                                Debug.Log($"Disabling feature set for {group}: {set.featureSetId}");
                            }
                        }

                        // Apply "no feature set" state and clear any enabled features.
                        OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(group);
                        ClearAllOpenXrFeatures(group);

                        var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
                        if (settings != null) EditorUtility.SetDirty(settings);
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    // Give OpenXR validation time to settle with no custom runtime loaders.
                    await WaitFramesAsync(2);

                    // Phase 2: Enable the desired feature set on the requested group only, apply, save.
                    targetSet.isEnabled = true;
                    OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(buildTargetGroup);

                    var finalSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
                    if (finalSettings != null) EditorUtility.SetDirty(finalSettings);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log($"Feature set '{targetSet.featureSetId}' enabled for '{buildTargetGroup}'.");
                }
                finally
                {
                    s_IsSwitchingFeatureSet = false;
                }
            };
        }

        /// <summary>
        /// Copy SDK setting files from the current package's SdkSettingFiles~ directory to the project's Assets directory.
        /// The SdkFolderName should match the folder name in the SdkSettingFiles~ directory.
        /// </summary>
        public static void CopySdkSettingFiles(string SdkFolderName)
        {
            var info = GetPackageInfo(GetCurrentPackageName());
            CopyFiles(
                Path.Combine(info.resolvedPath, "SdkSettingFiles~", SdkFolderName),
                Path.Combine(Application.dataPath)
            );
        }

        /// <summary>
        /// Copy files from source directory to destination directory, preserving directory structure.
        /// </summary>
        public static void CopyFiles(string sourceDirectory, string destinationDirectory)
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            foreach (var file in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
            {
                // Get the relative path from the source directory
                var relativePath = Path.GetRelativePath(sourceDirectory, file);
                var destFile = Path.Combine(destinationDirectory, relativePath);

                // Ensure the destination subdirectory exists
                var destDir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(file, destFile, true);
                UnityEngine.Debug.Log($"Copying file from {file} to {destFile}");
            }
        }

        /// <summary>
        /// Get the name of the current package name
        /// </summary>
        /// <returns>The name of the current package, or null if it cannot be determined.</returns>
        public static string GetCurrentPackageName()
        {
            var assembly = new System.Diagnostics.StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType?.Assembly;
            var packageInfo = assembly != null ? UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly) : null;
            return packageInfo?.name;
        }
        /// <summary>
        /// Get the version of an installed Unity package by its name.
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static string GetInstalledPackageVersion(string packageName)
        {
            var request = Client.List(true, true); // This requests the list of packages
            while (!request.IsCompleted) { } // Wait until the request is completed

            if (request.Status == StatusCode.Success)
            {
                foreach (var package in request.Result)
                {
                    if (package.name == packageName)
                    {
                        return package.version;
                    }
                }
            }
            else if (request.Status >= StatusCode.Failure)
            {
                UnityEngine.Debug.LogError("Failed to get package version.");
            }
            // Return an empty string or null if the package is not found
            return null;
        }

        /// <summary>
        /// Get package information by package name
        /// </summary>
        public static UnityEditor.PackageManager.PackageInfo GetPackageInfo(string packageName)
        {
            var request = Client.List(true, true);
            while (!request.IsCompleted) { }
            if (request.Status == StatusCode.Success) { return request.Result.FirstOrDefault(pkg => pkg.name == packageName); }
            return null;
        }

        /// <summary>
        /// Add a Unity package by its name and version.
        /// example: "com.unity.textmeshpro@3.0.6"
        /// </summary>
        /// <param name="packageIdentifier"></param>
        public static bool AddUnityPackage(string packageIdentifier)
        {
            // Exit if the packageIdentifier is null or empty
            if (string.IsNullOrEmpty(packageIdentifier)) { return false; }

            // Separate the package name and version
            var packageName = packageIdentifier.Split('@')[0];
            var version = packageIdentifier.Split('@').Length > 1 ? packageIdentifier.Split('@')[1] : null;

            // If the package is not Unity official and not a URL, add a scoped registry for OpenUPM
            if (!packageName.StartsWith("com.unity.") && !packageIdentifier.StartsWith("https://"))
            {
                AddScopedRegistryOfOpenUpmPackage(packageName);
            }

            // If the package is a tarball URL, download it and get the local file path
            packageIdentifier = DownloadAndGetLocalFilepathIfTarball(packageIdentifier);

            // Add the package and measure the time taken
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var request = UnityEditor.PackageManager.Client.Add(packageIdentifier);
            while (!request.IsCompleted) { }
            stopwatch.Stop();
            
            if (request.Error != null)
            {
                UnityEngine.Debug.LogError(request.Error.message);
                return false;
            }

            Debug.Log($"Package {packageIdentifier} added successfully.");
            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation(); // In order to invoke [DidReloadScripts] even when new version of the package is not installed.
            return true;
        }

        /// <summary>
        /// If the package is a tarball URL, download it and return the local file path. Returns the original packageIdentifier otherwise.
        /// </summary>
        private static string DownloadAndGetLocalFilepathIfTarball(string packageIdentifier)
        {
            string ret = packageIdentifier;

            // If the package is .tar.gz or .tgz located at a URL, download it to /Packages directory
            if ((packageIdentifier.EndsWith(".tar.gz") || packageIdentifier.EndsWith(".tgz")) && packageIdentifier.StartsWith("https://"))
            {
                string packageFileName = Path.GetFileName(packageIdentifier);
                string packagePath = Path.Combine("Packages", packageFileName);

                try
                {
                    if (File.Exists(packagePath)) File.Delete(packagePath);
                    using var wc = new System.Net.WebClient();
                    wc.DownloadFile(packageIdentifier, packagePath);
                    Debug.Log($"Downloaded package: {packagePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to download package: {e.Message}");
                    return null;
                }
                ret = "file:" + packageFileName; // Update the identifier to the local path.
            }
            return ret;
        }

        /// <summary>
        /// Check if a Unity package is installed by its name and version.
        /// </summary>
        /// <param name="packageName">The name of the package (e.g. "com.unity.textmeshpro").</param>
        /// <param name="version">The version of the package (e.g. "3.0.6").</param>
        /// <returns>
        /// True if the package is installed with the specified version, false otherwise.
        /// </returns>
        public static bool IsPackageInstalled(string packageName, string version)
        {
            if (string.IsNullOrEmpty(version)) { return false; }

            var request = UnityEditor.PackageManager.Client.List(true, true);
            while (!request.IsCompleted) { }
            if (request.Status == StatusCode.Success)
            {
                return request.Result.Any(pkg => pkg.name == packageName && pkg.version == version);
            }
            else if (request.Status >= StatusCode.Failure)
            {
                UnityEngine.Debug.LogError("Failed to check if package is installed.");
            }
            return false;
        }

        /// <summary>
        /// Remove a Unity package by its name.
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns>True if the package was successfully removed, false otherwise.</returns>
        public static bool RemoveUnityPackage(string packageName)
        {
            // Remove the package
            var request = UnityEditor.PackageManager.Client.Remove(packageName);
            while (!request.IsCompleted) { }
            if (request.Error != null)
            {
                UnityEngine.Debug.LogError(request.Error.message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Add a scoped registry of the OpenUPM package
        /// </summary>
        static void AddScopedRegistryOfOpenUpmPackage(string packageName)
        {
            AddScopedRegistry(new ScopedRegistry
            {
                name = "package.openupm.com",
                url = "https://package.openupm.com",
                scopes = new string[] {
                packageName
            }
            });
        }

        /// <summary>
        /// Add a scoped registry to the manifest.json file only if it doesn't already exist.
        /// </summary>
        static void AddScopedRegistry(ScopedRegistry pScopeRegistry)
        {
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages/manifest.json");
            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JsonConvert.DeserializeObject<ManifestJson>(manifestJson);
            var existingRegistry = manifest.scopedRegistries.FirstOrDefault(r => r.name == pScopeRegistry.name);

            if (existingRegistry != null)
            {
                // Check if the scope already exists
                if (!existingRegistry.scopes.Contains(pScopeRegistry.scopes[0]))
                {
                    // Add the new scope to the existing registry
                    var scopesList = existingRegistry.scopes.ToList();
                    scopesList.Add(pScopeRegistry.scopes[0]);
                    existingRegistry.scopes = scopesList.ToArray();
                }
            }
            else
            {
                // Add the new registry
                manifest.scopedRegistries.Add(pScopeRegistry);
            }
            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
        }

        /// <summary>
        /// Configure PICO hand tracking settings by directly modifying the PICOProjectSetting asset
        /// This enables the Hand Tracking checkbox whenever switching to the PICO SDK.
        /// </summary>
        public static void ConfigurePicoHandTracking()
        {
            EditorApplication.delayCall += () =>
            {
                try
                {
                    // Load the PICO project setting asset
                    var picoProjectSetting = Resources.Load("PICOProjectSetting");
                    if (picoProjectSetting != null)
                    {
                        ModifyPicoProjectSettingAsset(picoProjectSetting);
                        return;
                    }

                    Debug.LogWarning("PICOProjectSetting asset not found. Hand tracking configuration failed.");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to configure PICO hand tracking: {e.Message}");
                }
            };
        }

        /// <summary>
        /// Modify the PICOProjectSetting asset to enable hand tracking
        /// </summary>
        private static void ModifyPicoProjectSettingAsset(object picoProjectSetting)
        {
            try
            {
                var type = picoProjectSetting.GetType();

                // Set isHandTracking to true (handTrackingSupportType defaults to ControllersAndHands)
                const string IsHandTrackingFieldName = "isHandTracking";
                var isHandTrackingField = type.GetField(IsHandTrackingFieldName, BindingFlags.Public | BindingFlags.Instance);
                if (isHandTrackingField != null && isHandTrackingField.FieldType == typeof(bool))
                {
                    isHandTrackingField.SetValue(picoProjectSetting, true);
                    Debug.Log("Enabled PICO Hand Tracking.");
                }
                else
                {
                    Debug.LogWarning($"Could not find field '{IsHandTrackingFieldName}' or it has the wrong type in PICOProjectSetting asset. Hand tracking configuration may have failed.");
                }

                // Save changes
                if (picoProjectSetting is UnityEngine.Object unityObject)
                {
                    EditorUtility.SetDirty(unityObject);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to modify PICO hand tracking settings: {e.Message}");
            }
        }

        // Prevent overlapping transitions and define relevant groups for OpenXR feature set switching.
        static bool s_IsSwitchingFeatureSet = false;
        static readonly BuildTargetGroup[] s_RelevantGroups = new[]
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.Standalone
        };

        class ScopedRegistry
        {
            public string name;
            public string url;
            public string[] scopes;
        }

        class ManifestJson
        {
            public Dictionary<string, string> dependencies = new();
            public List<ScopedRegistry> scopedRegistries = new();
        }
    }
}