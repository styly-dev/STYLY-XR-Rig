using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Styly.XRRig
{
    public class MoveVolumeCameraToUnderCameraOffset : MonoBehaviour
    {
        [SerializeField]
        private GameObject CameraOffset;
        
        void Start()
        {
            MoveVolumeCameraToUnderCameraOffsetFunction();
        }

        void MoveVolumeCameraToUnderCameraOffsetFunction(){
            this.gameObject.transform.parent = CameraOffset.transform;
        }


    }
}