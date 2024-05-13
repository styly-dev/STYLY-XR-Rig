using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Styly.XRRig
{
    public class SetCameraYOffsetZeroOnBuildApp : MonoBehaviour
    {
        [SerializeField]
        private GameObject CameraOffset;

        void Awake()
        {
            // Skip if running in the editor
            if (Application.isEditor) { return; }

            // Set the camera offset to zero
            SetCameraOffsetY_Zero();
        }

        /// <summary>
        /// Set the camera offset to zero
        /// </summary>
        void SetCameraOffsetY_Zero()
        {
            CameraOffset.transform.localPosition = new Vector3(CameraOffset.transform.localPosition.x, 0, CameraOffset.transform.localPosition.z);
        }
    }
}