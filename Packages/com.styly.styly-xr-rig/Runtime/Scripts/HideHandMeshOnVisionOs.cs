using UnityEngine;

namespace Styly.XRRig
{
    /// <summary>
    /// Hide hand mesh on VisionOS
    /// </summary>
    public class HideHandMeshOnVisionOs : MonoBehaviour
    {
        [SerializeField]
        private GameObject LeftHandVisual;
        [SerializeField]
        private GameObject RightHandVisual;

        [SerializeField]
        private GameObject LeftPinchVisual;
        [SerializeField]
        private GameObject RightPinchVisual;

        void Awake()
        {
            RemoveMeshOfSkinnedMeshRenderer(LeftHandVisual);
            RemoveMeshOfSkinnedMeshRenderer(RightHandVisual);
            RemoveMeshOfSkinnedMeshRenderer(LeftPinchVisual);
            RemoveMeshOfSkinnedMeshRenderer(RightPinchVisual);
        }

        /// <summary>
        /// Remove mesh of SkinnedMeshRenderer
        /// </summary>
        void RemoveMeshOfSkinnedMeshRenderer(GameObject obj)
        {
            SkinnedMeshRenderer[] skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                skinnedMeshRenderer.sharedMesh = null;
            }
        }
    }
}