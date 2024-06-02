using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARAnchorManagerAttacher : MonoBehaviour
{
    [SerializeField] private GameObject anchorPrefab;

    private void Awake()
    {
        AttachToXrOrigin();
    }

    private void AttachToXrOrigin()
    {
        var xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin == null) return;
        // Add the ARAnchorManager component to the GameObject
        var arAnchorManager = xrOrigin.gameObject.AddComponent<ARAnchorManager>();
        // Disable the ARAnchorManager component
        arAnchorManager.enabled = false;
        // Set default anchor Prefab of the ARAnchorManager component
        arAnchorManager.anchorPrefab = anchorPrefab;
        // Debug log
        Debug.Log("ARAnchorManager is attached to XROrigin");
    }
}
