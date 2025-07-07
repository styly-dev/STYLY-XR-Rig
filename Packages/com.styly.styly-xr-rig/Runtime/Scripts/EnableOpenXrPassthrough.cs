using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Styly.XRRig
{
    /// <summary>
    /// Enable OpenXR Passthrough
    /// </summary>
    public class EnableOpenXrPassthrough : MonoBehaviour
    {
        void Awake()
        {
            // Skip on Vision OS
            if (Utils.IsVisionOS()) { return; }

            // Enable Passthrough
            EnablePassthrough();
        }

        public void EnablePassthrough()
        {
            // Find Main Camera of STYKY-XR-Rig
            var STYLYXRRig = GameObject.FindFirstObjectByType<Styly.XRRig.StylyXrRig>();
            var MainCamera = STYLYXRRig.transform.GetComponentsInChildren<Camera>().FirstOrDefault(camera => camera.CompareTag("MainCamera")).gameObject;
            // Set Main Camera parameteres
            MainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            // Set Main Camera Background to black with 0 alpha
            MainCamera.GetComponent<Camera>().backgroundColor = new Color(0, 0, 0, 0);
            // Set HDR Rendering to false
            MainCamera.GetComponent<Camera>().allowHDR = false;
            // Add AR Camera Manage
            MainCamera.AddComponent<ARCameraManager>();
            // Add AR Camera Background
            MainCamera.AddComponent<ARCameraBackground>();
            // Debug log
            Debug.Log("OpenXR Passthrough is setup");
        }
    }
}
