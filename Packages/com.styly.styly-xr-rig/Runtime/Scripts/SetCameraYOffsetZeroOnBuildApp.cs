using UnityEngine;

namespace Styly.XRRig
{
    /// <summary>
    /// Set the camera offset to zero on build app
    /// In the editor, the camera offset is set to CameraHeightInEditor.
    /// </summary>
    public class SetCameraYOffsetZeroOnBuildApp : MonoBehaviour
    {
        [SerializeField]
        private GameObject CameraOffset;

        void Awake()
        {
            // Skip if running in the editor
            if (Application.isEditor) { return; }
#if USE_XREAL
            // Skip if using XREAL
            return;
#endif
            // Set the camera offset to zero
            SetCameraOffsetY_Zero();
        }

        /// <summary>
        /// Set the camera offset to zero
        /// </summary>
        void SetCameraOffsetY_Zero()
        {
            CameraOffset.transform.localPosition = new Vector3(CameraOffset.transform.localPosition.x, 0, CameraOffset.transform.localPosition.z);
            Debug.Log("Set Camera Offset Y to Zero");
        }
    }
}