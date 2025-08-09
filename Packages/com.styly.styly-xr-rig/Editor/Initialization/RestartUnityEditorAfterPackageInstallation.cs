using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;

/// <summary>
/// Restart Unity Editor after package installation
/// </summary>
class RestartUnityEditorAfterPackageInstallation
{
    [InitializeOnLoadMethod]
    static void ExecuteOnceAfterPackageInstallation()
    {
        if (IsFirstRun.IsFirstRunForVersion())
        {
            if (EditorUtility.DisplayDialog("Restart Unity",
                "You need to restart Unity to apply the new changes. Restart now?",
                "Restart", "Later"))
            {
                // Restart Unity Editor
                EditorApplication.OpenProject(System.Environment.CurrentDirectory);
            }
            else
            {
                // Inform the user to restart Unity manually
                EditorUtility.DisplayDialog("Manual Restart Required",
                    "Please close and reopen Unity to complete the update.",
                    "OK");
            }
        }
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