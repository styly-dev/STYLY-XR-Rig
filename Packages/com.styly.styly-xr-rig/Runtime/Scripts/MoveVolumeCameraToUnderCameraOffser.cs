using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Styly.XRRig
{
    public class MoveVolumeCameraToUnderCameraOffser : MonoBehaviour
    {
        [SerializeField]
        private GameObject CameraOffset;
        
        void Start()
        {
            MoveVolumeCameraToUnderCameraOffserFunction();
        }

        void MoveVolumeCameraToUnderCameraOffserFunction(){
            this.gameObject.transform.parent = CameraOffset.transform;
        }


    }
}