using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.PolySpatial;

namespace Styly.XRRig
{
    public class VolumeCameraConfiguration : MonoBehaviour
    {
        public bool UseBoundedModeForVisionOs = false;
        public VolumeCameraWindowConfiguration BoundedVolumeCamera = null;
        public VolumeCameraWindowConfiguration UnBoundedVolumeCamera = null;

        // Start is called before the first frame update
        void Start()
        {
            SetVolumeCameraConfiguration(UseBoundedModeForVisionOs);
        }

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
    }
}