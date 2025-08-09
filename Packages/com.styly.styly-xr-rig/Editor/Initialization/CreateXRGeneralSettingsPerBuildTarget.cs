using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using System.IO;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.Management;

public class CreateXRGeneralSettingsPerBuildTarget
{
    [InitializeOnLoadMethod]
    static void ExecuteOnceAfterPackageInstallation()
    {
        if (IsFirstRun.IsFirstRunForVersion())
        {
            InitializeXrPluginManagementForAllPlatforms();
        }
    }

    /// <summary>
    /// Initializes XR Plug-in Management settings for all platforms.
    /// </summary>
    public static void InitializeXrPluginManagementForAllPlatforms()
    {
        InitializeXrPluginManagement(BuildTargetGroup.Standalone);
        InitializeXrPluginManagement(BuildTargetGroup.Android);
        InitializeXrPluginManagement(BuildTargetGroup.iOS);
        InitializeXrPluginManagement(BuildTargetGroup.VisionOS);
        AssetDatabase.SaveAssets();
        Debug.Log("XR Plug-in Management is initialized.");
    }

    /// <summary>
    /// Initializes XR Plug-in Management settings for a specific build target group.
    /// </summary>
    public static void InitializeXrPluginManagement(BuildTargetGroup group)
    {
        // 1) Obtain or create the per-build-target settings container
        XRGeneralSettingsPerBuildTarget pbt;
        if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out pbt) || pbt == null)
        {
            pbt = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            Directory.CreateDirectory("Assets/XR");
            var pbtPath = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";
            AssetDatabase.CreateAsset(pbt, pbtPath);
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, pbt, true);
        }

        // 2) Prepare XRGeneralSettings for the target group
        if (!pbt.HasSettingsForBuildTarget(group)) pbt.CreateDefaultSettingsForBuildTarget(group);
        var general = pbt.SettingsForBuildTarget(group);

        // 3) Prepare XRManagerSettings
        if (!pbt.HasManagerSettingsForBuildTarget(group)) pbt.CreateDefaultManagerSettingsForBuildTarget(group);
        var manager = pbt.ManagerSettingsForBuildTarget(group);

        // Link just in case (compatibility with older versions)
        if (general.Manager == null) general.Manager = manager;

        EditorUtility.SetDirty(pbt);
        EditorUtility.SetDirty(general);
        EditorUtility.SetDirty(manager);
    }

    /// <summary>
    /// Provides methods to determine if this is the first run after a package installation or upgrade.
    /// </summary>
    private static class IsFirstRun
    {
        /// <summary>
        /// Whether this is the first run (different from the saved version)
        /// </summary>
        internal static bool IsFirstRunForVersion()
        {
            string packageName = GetThisPackageName();
            string currentVersion = GetPackageVersion(packageName);
            string key = GetVersionConfigKey(packageName);
            bool isFirstRun = EditorUserSettings.GetConfigValue(key) != currentVersion;
            if (isFirstRun) { SaveCurrentPackageVersion(); }
            return isFirstRun;
        }

        /// <summary>
        /// Save the current package version
        /// The data will be saved in the /UserSettings/EditorUserSettings.asset
        /// </summary>
        private static void SaveCurrentPackageVersion()
        {
            string packageName = GetThisPackageName();
            string currentVersion = GetPackageVersion(packageName);
            string key = GetVersionConfigKey(packageName);
            EditorUserSettings.SetConfigValue(key, currentVersion);
        }

        /// <summary>
        /// Get the package name this script belongs to
        /// </summary>
        private static string GetThisPackageName()
        {
            var pkgInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Assembly);
            return pkgInfo.name;
        }

        /// <summary>
        /// Get the version of the package
        /// </summary>
        private static string GetPackageVersion(string packageName)
        {
            var request = Client.List(true, true);
            while (!request.IsCompleted) { }
            return request.Result.FirstOrDefault(package => package.name == packageName)?.version;
        }

        /// <summary>
        /// Build a unique key that includes the package name and the current class name
        /// </summary>
        private static string GetVersionConfigKey(string packageName)
        {
            var declaringType = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            return $"VersionOf_{packageName}_{declaringType?.FullName}";
        }
    }
}
