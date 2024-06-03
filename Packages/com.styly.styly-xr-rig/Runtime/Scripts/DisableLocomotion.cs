using UnityEngine;

namespace Styly.XRRig
{
    /// <summary>
    /// Disable Locomotion
    /// </summary>
    public class DisableLocomotion : MonoBehaviour
    {
        [SerializeField]
        private GameObject locomotionObject;

        void Awake()
        {
            locomotionObject.SetActive(false);
            Debug.Log("Locomotion is disabled");
        }
    }
}