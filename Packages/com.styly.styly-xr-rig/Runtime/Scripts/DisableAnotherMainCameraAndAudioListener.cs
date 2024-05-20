using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DisableAnotherMainCameraAndAudioListener : MonoBehaviour
{
    private Camera XrRigCamera = null;
    private AudioListener XrRigAudioListener = null;

    void Start()
    {
        // Find MainCamera and AudioListener attached to the STYLY-XR-Rig.
        FindMainCameraAndAudioListenerOfXrRig();

        // Disable all MainCameras and AudioListeners except the one attached to the XRRig.
        DisableMainCamerasExcludingXrRigCamera();
        DisableAudioListenersExcludingXrRigAudioListener();
    }

    void FindMainCameraAndAudioListenerOfXrRig()
    {
        var STYLYXRRig = GameObject.FindObjectOfType<Styly.XRRig.StylyXrRig>();
        XrRigCamera = STYLYXRRig.transform.GetComponentsInChildren<Camera>().FirstOrDefault(camera => camera.CompareTag("MainCamera"));
        XrRigAudioListener = STYLYXRRig.transform.GetComponentInChildren<AudioListener>();
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
