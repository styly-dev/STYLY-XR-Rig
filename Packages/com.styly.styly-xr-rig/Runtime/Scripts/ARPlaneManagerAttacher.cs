using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlaneManagerAttacher : MonoBehaviour
{

    [SerializeField]
    GameObject PlanePrefab;

    void Start()
    {
        AttachARPlaneManagerToXrOrigin();
    }

    void AttachARPlaneManagerToXrOrigin()
    {
        XROrigin xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin != null)
        {
            // Add the ARPlaneManager component to the GameObject
            ARPlaneManager arPlaneManager = xrOrigin.gameObject.AddComponent<ARPlaneManager>();
            // Disable the ARPlaneManager component
            arPlaneManager.enabled = false;
            // Set default Plane Prefab of the ARPlaneManager component
            arPlaneManager.planePrefab = PlanePrefab;
        }
    }




}
