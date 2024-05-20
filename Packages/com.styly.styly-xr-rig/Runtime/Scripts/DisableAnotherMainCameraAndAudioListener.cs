using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAnotherMainCameraAndAudioListener : MonoBehaviour
{
    [SerializeField]
    private Camera XrRigCamera = null;
    [SerializeField]
    private AudioListener XrRigAudioListener = null;

    void Start()
    {   
        // Disable all MainCameras and AudioListeners except the one attached to the XRRig.
        DisableMainCamerasExcludingXrRigCamera();
        DisableAudioListenersExcludingXrRigAudioListener();
    }

    void DisableMainCamerasExcludingXrRigCamera()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera camera in cameras)
        {
            if (camera != XrRigCamera && camera.CompareTag("MainCamera"))
            {
                camera.enabled = false;
                Debug.Log("Another MainCamera is disabled");
            }
        }
    }

    void DisableAudioListenersExcludingXrRigAudioListener()
    {
        AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
        foreach (AudioListener audioListener in audioListeners)
        {
            if (audioListener != XrRigAudioListener)
            {
                audioListener.enabled = false;
                Debug.Log("Another AudioListener is disabled");
            }
        }
    }



}
