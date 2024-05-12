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
        private GameObject BoundedGuideFrame = null;
        [SerializeField]
        private GameObject CameraOffset = null;
        [SerializeField]
        private float CameraHeightInEditor = 1.3f;

        // Start is called before the first frame update
        void Start()
        {
            // Set volume camera configuration
            SetVolumeCameraConfiguration(UseBoundedModeForVisionOs);

            // Disable GuideFrame on build app
            DisableGuideFrameOnBuildApp();
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

        /// <summary>
        /// Disable GuideFrame on build app. Only disable in Editor mode.
        /// </summary>
        void DisableGuideFrameOnBuildApp()
        {
            if (!Application.isEditor)
            {
                BoundedGuideFrame.SetActive(false);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// When UseBoundedModeForVisionOs is changed in the editor, update some configurations.
        /// </summary>
        void OnValidate()
        {
            if (UseBoundedModeForVisionOs)
            {
                // Set to Bounded configuration
                if (BoundedGuideFrame.activeSelf == false)
                {
                    // Enable BoundedGuideFrame
                    BoundedGuideFrame.SetActive(true);
                    // Changet the size of BoundedGuideFrame to the size of BoundedVolumeCamera
                    BoundedGuideFrame.transform.localScale = BoundedVolumeCamera.Dimensions;
                    // Print log
                    Debug.Log("Display BoundedGuideFrame: On");
                }

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
                // Set to UnBounded configuration
                if (BoundedGuideFrame.activeSelf == true)
                {
                    // Disable BoundedGuideFrame
                    BoundedGuideFrame.SetActive(false);
                    // Print log
                    Debug.Log("Display BoundedGuideFrame: Off");
                }

                // Set the camera offset to CameraHeightInEditor
                var newCameraOffSet = new Vector3(CameraOffset.transform.localPosition.x, CameraHeightInEditor, CameraOffset.transform.localPosition.z);
                if (CameraOffset.transform.localPosition != newCameraOffSet)
                {
                    CameraOffset.transform.localPosition = newCameraOffSet;
                    Debug.Log("Changed CameraOffset: " + CameraOffset.transform.localPosition);
                }
            }
        }
# endif
    }
}