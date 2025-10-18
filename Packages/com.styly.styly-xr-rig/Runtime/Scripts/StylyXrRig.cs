using UnityEngine;
#if UNITY_VISIONOS && USE_POLYSPATIAL
using Unity.PolySpatial;
#endif

namespace Styly.XRRig
{
    public class StylyXrRig : MonoBehaviour
    {
        [SerializeField] private bool passthroughMode = true;
        private PassthroughManager passthroughManager;
        private SmartphoneARCameraManager smartphoneArCameraManager;

        public bool PassthroughMode => passthroughManager != null ? passthroughManager.PassthroughMode : passthroughMode;

        public void SwitchToVR(float duration = 1)
        {
            if (passthroughManager != null) { passthroughManager.SwitchToVR(duration); }
            if (smartphoneArCameraManager != null) { smartphoneArCameraManager.ConfigureOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForVR); }
        }

        public void SwitchToMR(float duration = 1)
        {
            if (passthroughManager != null) { passthroughManager.SwitchToMR(duration); }
            if (smartphoneArCameraManager != null) { smartphoneArCameraManager.ConfigureOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR); }
        }

#if UNITY_VISIONOS && USE_POLYSPATIAL
        [SerializeField]
        private bool UseBoundedModeForVisionOs = false;
        private GameObject VolumeCamera = null;
        [SerializeField]
        private VolumeCameraWindowConfiguration BoundedVolumeCamera = null;
        [SerializeField]
        private VolumeCameraWindowConfiguration UnBoundedVolumeCamera = null;
        [SerializeField]
        private GameObject CameraOffset = null;
        [SerializeField]
        private float CameraHeightInEditor = 1.3f;

        // Parameters of Bounded Guide Frame Gizmo
        private Vector3 DefaultBoundedGuideFrameGizmoSize = new(1, 1, 1);
        private Color BoundedGuideFrameGizmoColor = Color.yellow;

        void CreateVolumeCamera()
        {
            VolumeCamera volumeCamera;

            if (volumeCamera = FindFirstObjectByType<VolumeCamera>())
            {
                VolumeCamera = volumeCamera.gameObject;
                Debug.Log("Found existing VolumeCamera GameObject: " + VolumeCamera.name);
            }
            else
            {
                VolumeCamera = new GameObject("VolumeCamera");
                VolumeCamera.transform.SetParent(this.transform, false);
                volumeCamera = VolumeCamera.AddComponent<VolumeCamera>();
                volumeCamera.Dimensions = new Vector3(1, 1, 1);
                Debug.Log("Created VolumeCamera GameObject");
            }
        }


        /// <summary>
        /// Set volume camera configuration to VolumeCamera
        /// </summary>
        void SetVolumeCameraConfiguration()
        {
            VolumeCamera volumeCamera = VolumeCamera.GetComponent<VolumeCamera>();
            if (UseBoundedModeForVisionOs)
            {
                volumeCamera.WindowConfiguration = BoundedVolumeCamera;
            }
            else
            {
                volumeCamera.WindowConfiguration = UnBoundedVolumeCamera;
            }
        }

        /// <summary>
        /// Set the position of the Bounded volume camera to (0,0,0).
        /// </summary>
        void SetBoundedVolumeCameraPositionToZero()
        {
            if (UseBoundedModeForVisionOs)
            {
                VolumeCamera.transform.position = Vector3.zero;
                Debug.Log("Set Bounded Volume Camera Position to (0,0,0)");
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
                // --- Bounded Mode ---

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
                // --- Unbounded Mode ---

                // Update the Bounded Guide Frame Gizmo size
                DefaultBoundedGuideFrameGizmoSize = BoundedVolumeCamera.Dimensions;

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
                // --- Bounded Mode ---

                // Draw Bounded Guide Frame Gizmo always at 0,0,0
                Gizmos.color = BoundedGuideFrameGizmoColor;
                Gizmos.DrawWireCube(new Vector3(0, 0, 0), DefaultBoundedGuideFrameGizmoSize);
            }
        }

#endif
#endif
        private void AwakeForVisionOS()
        {
#if UNITY_VISIONOS && USE_POLYSPATIAL
            // Create VolumeCamera if it does not exist
            CreateVolumeCamera();
            // Set volume camera configuration
            SetVolumeCameraConfiguration();
            // Set the position of volume camera to (0,0,0) when Bounded mode
            SetBoundedVolumeCameraPositionToZero();
#endif
        }

        void Awake()
        {
            AwakeForVisionOS();
            passthroughManager = GetComponentInChildren<PassthroughManager>(false);
            smartphoneArCameraManager = GetComponentInChildren<SmartphoneARCameraManager>(false);
        }

        void Start()
        {
            if (passthroughManager != null)
            {
                if (passthroughMode)
                {
                    passthroughManager.SwitchToMR(0);
                }
                else
                {
                    passthroughManager.SwitchToVR(0);
                }
            }

            if (smartphoneArCameraManager != null)
            {
                if (passthroughMode)
                {
                    smartphoneArCameraManager.ConfigureOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR);
                }
                else
                {
                    smartphoneArCameraManager.ConfigureOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForVR);
                }
            }
        }
    }
}