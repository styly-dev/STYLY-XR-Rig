using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Styly.XRRig
{
    /// <summary>
    /// Attach ARTrackedImageManager to XROrigin.
    /// This script is created for maintenance reasons instead of modifying the original prefab.
    /// (ARTrackedImageManager can only be attached to XROrigin)
    /// </summary>
    public class ARTrackedImageManagerAttacher : MonoBehaviour
    {
        void Awake()
        {
            AttachARTrackedImageManagerToXrOrigin();
        }

        void AttachARTrackedImageManagerToXrOrigin()
        {
            XROrigin xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin != null)
            {
                // Create a cube to track the image with the size of 0.01, 0.01, 0.01
                GameObject trackedImageCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trackedImageCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                // Add the ARTrackedImageManager component to the GameObject
                ARTrackedImageManager arTrackedImageManager = xrOrigin.gameObject.AddComponent<ARTrackedImageManager>();

                // // Set up default parameters
                arTrackedImageManager.referenceLibrary = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
                arTrackedImageManager.requestedMaxNumberOfMovingImages = 1;
                arTrackedImageManager.trackedImagePrefab = trackedImageCube;

                // Disable the ARTrackedImageManager component
                arTrackedImageManager.enabled = false;

                // Debug log
                Debug.Log("ARTrackedImageManager is attached to XROrigin");
            }
        }
    }
}