using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

[assembly: InternalsVisibleTo("com.styly.styly-xr-rig.AndroidXR.Tests")]

namespace Styly.XRRig
{
    /// <summary>
    /// Requests permissions only for an active Android XR provider. Occlusion managers must
    /// remain disabled until scene-understanding permission is granted.
    /// https://developer.android.com/develop/xr/permissions
    /// </summary>
    public static class RequestAndroidXRPermissions
    {
        public const string HandTracking = "android.permission.HAND_TRACKING";
        public const string SceneUnderstandingFine = "android.permission.SCENE_UNDERSTANDING_FINE";

        private static readonly string[] Permissions =
        {
            HandTracking,
            SceneUnderstandingFine
        };

        internal static IAndroidXRPermissions Backend = new AndroidXRPermissions();
        internal static event Action<string> Granted;
        internal static bool IsAndroidXR => Backend.IsAndroidXR;
        private static bool requested;

        /// <summary>
        /// Returns true when permission is granted or the active provider is not Android XR.
        /// ARCore and other Android providers do not require Android XR permissions.
        /// </summary>
        public static bool IsGranted(string permission)
        {
            return !IsAndroidXR || Backend.IsGranted(permission);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetSession()
        {
            Backend = new AndroidXRPermissions();
            Granted = null;
            requested = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void RequestPermissions()
        {
            if (!IsAndroidXR || requested) { return; }

            var toRequest = new List<string>();
            foreach (var permission in Permissions)
            {
                if (!Backend.IsGranted(permission))
                {
                    toRequest.Add(permission);
                }
            }

            requested = true;
            if (toRequest.Count > 0)
            {
                Backend.Request(toRequest.ToArray(), OnGranted, OnDenied);
            }
        }

        private static void OnGranted(string permissionName)
        {
            Debug.Log($"Android XR permission granted: {permissionName}");
            Granted?.Invoke(permissionName);
        }

        private static void OnDenied(string permissionName)
        {
            Debug.LogWarning($"Android XR permission denied ({permissionName}). Features depending " +
                "on it will be disabled or degraded rather than crash.");
        }
    }

    internal interface IAndroidXRPermissions
    {
        bool IsAndroidXR { get; }
        bool IsGranted(string permission);
        void Request(string[] permissions, Action<string> onGranted, Action<string> onDenied);
    }

    internal sealed class AndroidXRPermissions : IAndroidXRPermissions
    {
        public bool IsAndroidXR
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                var settings = XRGeneralSettings.Instance;
                var manager = settings != null ? settings.Manager : null;
                return manager != null && UsesAndroidXR(manager.activeLoader);
#else
                return false;
#endif
            }
        }

        internal static bool UsesAndroidXR(XRLoader loader)
        {
            if (loader == null) { return false; }

            // Identify the loaded provider without requiring the optional Android XR assembly.
            var session = loader.GetLoadedSubsystem<XRSessionSubsystem>();
            var occlusion = loader.GetLoadedSubsystem<XROcclusionSubsystem>();
            return (session != null && session.subsystemDescriptor.id == "Android-Session")
                || (occlusion != null && occlusion.subsystemDescriptor.id == "Android-Occlusion");
        }

        public bool IsGranted(string permission)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(permission);
#else
            return true;
#endif
        }

        public void Request(string[] permissions, Action<string> onGranted, Action<string> onDenied)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Android callbacks must not access Unity objects from a platform callback thread.
            var unityContext = SynchronizationContext.Current;
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += permission => unityContext.Post(_ => onGranted(permission), null);
            callbacks.PermissionDenied += permission => unityContext.Post(_ => onDenied(permission), null);
            callbacks.PermissionDeniedAndDontAskAgain += permission => unityContext.Post(_ => onDenied(permission), null);
            Permission.RequestUserPermissions(permissions, callbacks);
#endif
        }
    }
}
