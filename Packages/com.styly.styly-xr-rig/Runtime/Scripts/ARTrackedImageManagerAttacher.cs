using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;

namespace Styly.XRRig
{
    /// <summary>
    /// Attach ARTrackedImageManager to XROrigin.
    /// This script is created for maintenance reasons instead of modifying the original prefab.
    /// (ARTrackedImageManager can only be attached to XROrigin)
    /// </summary>
    public class ARTrackedImageManagerAttacher : MonoBehaviour
    {
        void Start()
        {
            AttachARTrackedImageManagerToXrOrigin();
        }

        void AttachARTrackedImageManagerToXrOrigin()
        {
            XROrigin xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin != null)
            {
                // Add the ARTrackedImageManager component to the GameObject
                ARTrackedImageManager arTrackedImageManager = xrOrigin.gameObject.AddComponent<ARTrackedImageManager>();
                // Disable the ARTrackedImageManager component
                arTrackedImageManager.enabled = false;
                // Debug log
                Debug.Log("ARTrackedImageManager is attached to XROrigin");
            }
        }
    }
}