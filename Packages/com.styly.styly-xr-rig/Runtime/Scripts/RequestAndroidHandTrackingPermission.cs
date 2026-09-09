using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace Styly.XRRig
{
    /// <summary>
    /// Android XR gates hand tracking data behind the runtime permission
    /// android.permission.HAND_TRACKING (similar to Camera/Microphone). Enabling the
    /// OpenXR Hand Tracking Subsystem feature only declares the capability; without this
    /// permission being granted, the OS withholds joint data and the system falls back to
    /// its own default controller-like input, even though no controller hardware exists.
    /// https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.4/manual/features/hand-tracking.html
    /// </summary>
    public static class RequestAndroidHandTrackingPermission
    {
        private const string HandTrackingPermission = "android.permission.HAND_TRACKING";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RequestPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(HandTrackingPermission))
            {
                return;
            }

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += OnGranted;
            callbacks.PermissionDenied += OnDenied;
            callbacks.PermissionDeniedAndDontAskAgain += OnDenied;
            Permission.RequestUserPermissions(new[] { HandTrackingPermission }, callbacks);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void OnGranted(string permissionName)
        {
            Debug.Log($"Hand tracking permission granted: {permissionName}");
        }

        private static void OnDenied(string permissionName)
        {
            Debug.LogWarning($"Hand tracking permission denied ({permissionName}). Hands will not be " +
                "tracked until this is granted in the device's Android app settings.");
        }
#endif
    }
}
