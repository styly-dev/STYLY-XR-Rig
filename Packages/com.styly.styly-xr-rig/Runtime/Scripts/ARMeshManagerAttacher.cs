using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;

namespace Styly.XRRig
{
    /// <summary>
    /// Attach ARMeshManager to XROrigin.
    /// This script is created for maintenance reasons instead of modifying the original prefab.
    /// </summary>
    public class ARMeshManagerAttacher : MonoBehaviour
    {
        // Default mesh prefab to attach ARMeshManager
        public MeshFilter DefaultMeshPrefab;

        void Start()
        {
            AttachARMeshManagerToXrOrigin();
        }

        void AttachARMeshManagerToXrOrigin()
        {
            XROrigin xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin != null)
            {
                // Create new GameObject named "ARMeshManager" as a child of XROrigin
                GameObject arMeshManagerGameObject = new("ARMeshManager");
                arMeshManagerGameObject.transform.SetParent(xrOrigin.transform);
                // Add the ARMeshManager component to the GameObject
                ARMeshManager arMeshManagerAttacherManager = arMeshManagerGameObject.AddComponent<ARMeshManager>();
                // Set the meshPrefab to the ARMeshManager component
                arMeshManagerAttacherManager.meshPrefab = DefaultMeshPrefab;
                // Disable the ARMeshManager component
                arMeshManagerAttacherManager.enabled = false;
                // Debug log
                Debug.Log("ARMeshManager is attached to XROrigin");
            }
        }
    }
}