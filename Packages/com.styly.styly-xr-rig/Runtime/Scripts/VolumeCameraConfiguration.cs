using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.PolySpatial;

namespace Styly.XRRig
{
    public class VolumeCameraConfiguration : MonoBehaviour
    {
        [SerializeField]
        private bool UseBoundedModeForVisionOs = false;
        [SerializeField]
        private VolumeCameraWindowConfiguration BoundedVolumeCamera = null;
        [SerializeField]
        private VolumeCameraWindowConfiguration UnBoundedVolumeCamera = null;
        [SerializeField]
        private GameObject CameraOffset = null;
        [SerializeField]
        private float CameraHeightInEditor = 1.3f;

        // Parameters of Bounded Guide Frame Gizmo
        private Vector3 BoundedGuideFrameGizmoSize = new(1, 1, 1);
        private Color BoundedGuideFrameGizmoColor = Color.yellow;

        // Start is called before the first frame update
        void Start()
        {
            // Set volume camera configuration
            SetVolumeCameraConfiguration(UseBoundedModeForVisionOs);
        }

        /// <summary>
        /// Set volume camera configuration to VolumeCamera
        /// </summary>
        void SetVolumeCameraConfiguration(bool useBoundedMode)
        {
            VolumeCamera volumeCamera = this.GetComponentInChildren<VolumeCamera>(true);
            if (useBoundedMode)
            {
                volumeCamera.WindowConfiguration = BoundedVolumeCamera;
            }
            else
            {
                volumeCamera.WindowConfiguration = UnBoundedVolumeCamera;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// When UseBoundedModeForVisionOs is changed in the editor, update some configurations.
        /// </summary>
        void OnValidate()
        {
            // Null check
            if (CameraOffset == null) { return; }

            if (UseBoundedModeForVisionOs)
            {
                // Set the camera offset to zero
                var newCameraOffSet = new Vector3(CameraOffset.transform.localPosition.x, 0, CameraOffset.transform.localPosition.z);
                if (CameraOffset.transform.localPosition != newCameraOffSet)
                {
                    CameraOffset.transform.localPosition = newCameraOffSet;
                    Debug.Log("Changed CameraOffset: " + CameraOffset.transform.localPosition);
                }
            }
            else
            {
                // Update the Bounded Guide Frame Gizmo size
                BoundedGuideFrameGizmoSize = BoundedVolumeCamera.Dimensions;

                // Set the camera offset to CameraHeightInEditor
                var newCameraOffSet = new Vector3(CameraOffset.transform.localPosition.x, CameraHeightInEditor, CameraOffset.transform.localPosition.z);
                if (CameraOffset.transform.localPosition != newCameraOffSet)
                {
                    CameraOffset.transform.localPosition = newCameraOffSet;
                    Debug.Log("Changed CameraOffset: " + CameraOffset.transform.localPosition);
                }
            }
        }

        /// <summary>
        /// Draw Bounded Guide Frame Gizmo
        /// </summary>
        void OnDrawGizmos()
        {
            if (UseBoundedModeForVisionOs)
            {
                Gizmos.color = BoundedGuideFrameGizmoColor;
                Gizmos.DrawWireCube(transform.position, BoundedGuideFrameGizmoSize);
            }
        }
# endif
    }
}