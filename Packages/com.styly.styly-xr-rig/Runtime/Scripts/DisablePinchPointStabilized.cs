using UnityEngine;

namespace Styly.XRRig
{
    /// <summary>
    /// Disable pointer from the finger
    /// </summary>
    public class DisablePinchPointStabilized : MonoBehaviour
    {
        [SerializeField]
        private GameObject LeftPinchPointStabilizedObject;
        [SerializeField]
        private GameObject RightPinchPointStabilizedObject;


        void Awake()
        {
            LeftPinchPointStabilizedObject.SetActive(false);
            RightPinchPointStabilizedObject.SetActive(false);
            Debug.Log("PinchPointStabilized is disabled");
        }
    }
}