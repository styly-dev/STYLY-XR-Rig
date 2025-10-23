using UnityEngine;

namespace Styly.XRRig
{
    /// <summary>
    /// Hide hand mesh on VisionOS
    /// </summary>
    public class HideHandMeshOnVisionOs : MonoBehaviour
    {
        [SerializeField]
        private Material AROcclusionMaterial;

        [SerializeField]
        private GameObject LeftHandVisual;
        [SerializeField]
        private GameObject RightHandVisual;

        [SerializeField]
        private GameObject LeftPinchVisual;
        [SerializeField]
        private GameObject RightPinchVisual;

        void Start()
        {
            if (Utils.IsVisionOS())
            {
                // Apply AR Occlusion to both hands
                ChangeMaterials(LeftHandVisual, AROcclusionMaterial);
                ChangeMaterials(RightHandVisual, AROcclusionMaterial);

                // Remove mesh of pinch visuals
                RemoveMeshOfSkinnedMeshRenderer(LeftPinchVisual);
                RemoveMeshOfSkinnedMeshRenderer(RightPinchVisual);
            }
        }

        /// <summary>
        /// Change materials of GameObject
        /// </summary>
        void ChangeMaterials(GameObject obj, Material material)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }
                renderer.materials = materials;
            }
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