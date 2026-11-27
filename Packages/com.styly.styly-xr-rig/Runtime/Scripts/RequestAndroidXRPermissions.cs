using System;
using System.Linq;
using System.Threading;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR && USE_ANDROIDXR
using UnityEngine.Android;
#endif

namespace Styly.XRRig
{
    /// <summary>
    /// Requests Android XR runtime permissions at startup. Occlusion managers must
    /// remain disabled until scene-understanding permission is granted.
    /// https://developer.android.com/develop/xr/permissions
    /// </summary>
    public static class RequestAndroidXRPermissions
    {
        public const string HandTracking = "android.permission.HAND_TRACKING";
        public const string SceneUnderstandingFine = "android.permission.SCENE_UNDERSTANDING_FINE";

        /// <summary>Raised on the Unity main thread when a permission is granted.</summary>
        internal static event Action<string> Granted;

        /// <summary>True in Android player builds that include the Android XR package.</summary>
        internal static bool IsAndroidXR
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR && USE_ANDROIDXR
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Returns true when permission is granted or the build is not Android XR.
        /// </summary>
        public static bool IsGranted(string permission)
        {
#if UNITY_ANDROID && !UNITY_EDITOR && USE_ANDROIDXR
            return Permission.HasUserAuthorizedPermission(permission);
#else
            return true;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RequestPermissions()
        {
#if UNITY_ANDROID && !UNITY_EDITOR && USE_ANDROIDXR
            var toRequest = new[] { HandTracking, SceneUnderstandingFine }.Where(p => !IsGranted(p)).ToArray();
            if (toRequest.Length == 0) { return; }

            // Permission callbacks run on an Android thread; raise Granted on the Unity thread.
            var unityContext = SynchronizationContext.Current;
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += p => unityContext?.Post(_ => Granted?.Invoke(p), null);
            callbacks.PermissionDenied += p => Debug.LogWarning($"Android XR permission denied: {p}");
            callbacks.PermissionDeniedAndDontAskAgain += p => Debug.LogWarning($"Android XR permission denied: {p}");
            Permission.RequestUserPermissions(toRequest, callbacks);
#endif
        }
    }
}
