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

namespace Styly.XRRig.SdkSwitcher
{
    public class SwitchSdkUtils
    {
        /// <summary>
        /// Clears all OpenXR features for the specified build target group.
        /// This will disable all features, effectively resetting the OpenXR settings for that build target group
        /// </summary>
        public static void ClearAllOpenXrFeatures(BuildTargetGroup buildTargetGroup)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
            foreach (var feature in settings.GetFeatures())
            {
                // Disable the feature
                feature.enabled = false;
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
        /// Data will be saved to `Assets/XR/Settings/OpenXR Editor Settings.asset`
        /// </summary>
        /// <param name="featureSetId">
        /// The ID of the feature set to enable (e.g. "com.unity.openxr.featureset.meta").
        /// </param>
        /// <param name="buildTargetGroup">The build target group for which to enable the feature set.</param>
        /// <example>
        /// EnableXRFeatureSet("com.unity.openxr.featureset.meta", BuildTargetGroup.Android);
        /// </example>
        public static void EnableXRFeatureSet(string featureSetId, BuildTargetGroup buildTargetGroup)
        {
            var featureSet = OpenXRFeatureSetManager.FeatureSetsForBuildTarget(buildTargetGroup).FirstOrDefault((set => set.featureSetId == featureSetId));
            if (featureSet == null)
            {
                Debug.LogError($"Feature set with ID '{featureSetId}' not found for build target group '{buildTargetGroup}'.");
                return;
            }

            if (!featureSet.isInstalled)
            {
                Debug.LogError($"Feature set with ID '{featureSetId}' is not installed.");
                return;
            }

            // Disable all other feature sets for the build target group
            foreach (var set in OpenXRFeatureSetManager.FeatureSetsForBuildTarget(buildTargetGroup))
            {
                if (set.featureSetId != featureSetId)
                {
                    set.isEnabled = false;
                    Debug.Log($"Disabling feature set: {set.featureSetId}");
                }
            }

            // Enable the specified feature set
            featureSet.isEnabled = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(buildTargetGroup);
            AssetDatabase.SaveAssets();

            Debug.Log($"Feature set '{featureSet.featureSetId}' has been enabled for build target group '{buildTargetGroup}'.");

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
        /// <param name="packageNameWithVersion"></param>
        /// <returns></returns>
        public static bool AddUnityPackage(string packageNameWithVersion)
        {
            // Separate the package name and version
            var packageName = packageNameWithVersion.Split('@')[0];
            var version = packageNameWithVersion.Split('@').Length > 1 ? packageNameWithVersion.Split('@')[1] : null;

            // If the package is not Unity official and not a URL, add a scoped registry for OpenUPM
            if (!packageName.StartsWith("com.unity.") && !packageName.StartsWith("https://"))
            {
                AddScopedRegistryOfOpenUpmPackage(packageName);
            }

            // Add the package
            var request = UnityEditor.PackageManager.Client.Add(packageNameWithVersion);
            while (!request.IsCompleted) { }
            if (request.Error != null)
            {
                UnityEngine.Debug.LogError(request.Error.message);
                return false;
            }
            return true;
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