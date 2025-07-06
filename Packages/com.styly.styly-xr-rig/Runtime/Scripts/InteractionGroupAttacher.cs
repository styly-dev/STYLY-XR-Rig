using UnityEngine;

namespace Styly.XRRig
{
        public class InteractionGroupAttacher : MonoBehaviour
        {
                // Set PrimaryInteractionGroup prefab from XRI visionOS sample
                [SerializeField]
                private GameObject PrimaryInteractionGroup;

                // Set SecondaryInteractionGroupVariant prefab from XRI visionOS sample
                [SerializeField]
                private GameObject SecondaryInteractionGroupVariant;

                // Set _added GameObject inside STYLY XR Rig
                [SerializeField]
                private GameObject AttachTo;

                void Awake()
                {
#if UNITY_VISIONOS && USE_POLYSPATIAL
                AttachInteractionGroups();
#endif
                }

                void AttachInteractionGroups()
                {
                        // Add primary interaction groups to the AttachTo GameObject
                        GameObject primaryGroupInstance = Instantiate(PrimaryInteractionGroup, AttachTo.transform);
                        primaryGroupInstance.name = PrimaryInteractionGroup.name;

                        // Add secondary interaction groups to the AttachTo GameObject
                        GameObject secondaryGroupInstance = Instantiate(SecondaryInteractionGroupVariant, AttachTo.transform);
                        secondaryGroupInstance.name = SecondaryInteractionGroupVariant.name;

                        Debug.Log("Interaction groups attached to " + AttachTo.name);
                }
        }
}