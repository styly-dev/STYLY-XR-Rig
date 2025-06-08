using System.Collections.Generic;
#if UNITY_VISIONOS && USE_POLYSPATIAL
using Unity.PolySpatial;
#endif
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Styly.XRRig
{
    /// <summary>
    /// Add VisionOSHoverEffect to the object which has XRGrabInteractable or XRSimpleInteractable
    /// </summary>
    public class AddVisionOSHoverEffect : MonoBehaviour
    {
#if UNITY_VISIONOS && USE_POLYSPATIAL
        void Start()
        {
            if (Utils.IsVisionOS())
            {
                AttachHoverEffectToInteractableGameObjects();
            }
        }

        /// <summary>
        /// Attach VisionOSHoverEffect to the interactable GameObjects
        /// </summary>
        void AttachHoverEffectToInteractableGameObjects()
        {
            int attachedCount = 0;
            var interactableGameObjects = GetInteractableGameObjects();
            foreach (var interactableGameObject in interactableGameObjects)
            {
                var hoverEffect = interactableGameObject.GetComponent<VisionOSHoverEffect>();
                if (hoverEffect == null)
                {
                    hoverEffect = interactableGameObject.AddComponent<VisionOSHoverEffect>();
                    attachedCount++;
                }
            }
            if (attachedCount > 0)
            {
                Debug.Log($"Attached {attachedCount} VisionOSHoverEffect to interactable GameObjects");
            }
        }

        /// <summary>
        /// Get interactable GameObjects which have XRGrabInteractable or XRSimpleInteractable
        /// </summary>
        GameObject[] GetInteractableGameObjects()
        {
            var interactableGameObjects = new List<GameObject>();

            var interactables = GameObject.FindObjectsOfType<XRGrabInteractable>(true);
            foreach (var interactable in interactables)
            {
                interactableGameObjects.Add(interactable.gameObject);
            }

            var simpleInteractables = GameObject.FindObjectsOfType<XRSimpleInteractable>(true);
            foreach (var simpleInteractable in simpleInteractables)
            {
                interactableGameObjects.Add(simpleInteractable.gameObject);
            }

            return interactableGameObjects.ToArray();
        }



#endif
    }
}
