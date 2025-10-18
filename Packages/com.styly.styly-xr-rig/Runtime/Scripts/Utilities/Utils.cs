using System.Linq;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Styly.XRRig
{
    public class Utils
    {
        /// <summary>
        /// Return true if current OS is VisionOS.
        /// </summary>
        /// <returns></returns>
        public static bool IsVisionOS()
        {
            //When operatingSystem is "visionOS", return true
            if (SystemInfo.operatingSystem.Contains("visionOS")) return true;
            if (Application.platform == RuntimePlatform.VisionOS) return true;
#if UNITY_VISIONOS
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Get the list of active XR Plugin provider IDs.
        /// This returns provider IDs for Standalone Desktop OS even when the build target is set to another platform.
        /// </summary>
        /// <example>
        /// Unity.XR.OpenXR, UnityEngine.XR.ARCore.ARCoreLoader, UnityEngine.XR.ARKit.ARKitLoader, UnityEngine.XR.VisionOS.VisionOSLoader
        /// </example>
        /// <returns></returns>
        public static string[] GetActiveXrPluginProviderIds()
        {
            var instance = XRGeneralSettings.Instance;
            var mgr = instance != null ? instance.Manager : null;
            if (mgr == null) return System.Array.Empty<string>();

            return mgr.activeLoaders
                .Where(l => l != null)
                .Select(l => l.GetType().Assembly.GetName().Name)
                .Distinct()
                .ToArray();
        }

    }
}