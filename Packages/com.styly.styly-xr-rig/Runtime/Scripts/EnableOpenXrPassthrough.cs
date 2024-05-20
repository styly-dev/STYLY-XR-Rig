using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Styly.XRRig
{
    public class EnableOpenXrPassthrough : MonoBehaviour
    {
        [SerializeField]
        private GameObject MainCamera;

        void Start()
        {
            // Skip if Vision OS
            if (Utils.IsVisionOS()) { return; }

            // Enable Passthrough
            EnablePassthrough();
        }

        public void EnablePassthrough()
        {
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
