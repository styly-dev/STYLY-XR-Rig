using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace Styly.XRRig
{
    /// <summary>
    /// Android XR gates several capabilities behind dangerous runtime permissions that are not
    /// requested just because the corresponding OpenXR feature is enabled (similar to
    /// Camera/Microphone). Missing one doesn't just disable the feature quietly: on Android XR,
    /// requesting environment depth without android.permission.SCENE_UNDERSTANDING_FINE granted
    /// makes xrCreateDepthSwapchainANDROID fail with XR_ERROR_PERMISSION_INSUFFICIENT, which goes
    /// on to crash the whole OpenXR session a few seconds later (VK_ERROR_DEVICE_LOST). Request
    /// every such permission once at startup, and let features check <see cref="IsGranted"/> to
    /// degrade gracefully instead of relying on the OS having already granted it.
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

        /// <summary>
        /// Whether the given Android XR runtime permission has been granted. On platforms where
        /// this concept doesn't apply (Editor, non-Android), always returns true so callers don't
        /// need their own platform guards.
        /// </summary>
        public static bool IsGranted(string permission)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(permission);
#else
            return true;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RequestPermissions()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var toRequest = new System.Collections.Generic.List<string>();
            foreach (var permission in Permissions)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    toRequest.Add(permission);
                }
            }

            if (toRequest.Count == 0) { return; }

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += OnGranted;
            callbacks.PermissionDenied += OnDenied;
            callbacks.PermissionDeniedAndDontAskAgain += OnDenied;
            Permission.RequestUserPermissions(toRequest.ToArray(), callbacks);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void OnGranted(string permissionName)
        {
            Debug.Log($"Android XR permission granted: {permissionName}");
        }

        private static void OnDenied(string permissionName)
        {
            Debug.LogWarning($"Android XR permission denied ({permissionName}). Features depending " +
                "on it will be disabled or degraded rather than crash.");
        }
#endif
    }
}
