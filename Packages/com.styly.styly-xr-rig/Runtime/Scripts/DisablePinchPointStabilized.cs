using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Hands;

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

        
        [SerializeField] 
        private GameObject LeftHandLineVisual;
        [SerializeField]
        private GameObject RightHandLineVisual;
        
        [SerializeField]
        private XRHandTrackingEvents m_MetaQuestLeftHandTrackingEvents;

        [SerializeField]
        private XRHandTrackingEvents m_MetaQuestRightHandTrackingEvents;

        [SerializeField]
        private XRHandTrackingEvents m_AndroidXRLeftHandTrackingEvents;

        [SerializeField]
        private XRHandTrackingEvents m_AndroidXRRightHandTrackingEvents;
        
        void Start()
        {
            // Clear jointsUpdated persistent/runtime listeners before execution starts
            ClearJointsUpdatedListeners();

            // Disable target objects first so even if events fire, nothing happens because targets are inactive
            if (LeftPinchPointStabilizedObject != null) 
            {
                LeftPinchPointStabilizedObject.SetActive(false);
            }
            if (RightPinchPointStabilizedObject != null) 
            {
                RightPinchPointStabilizedObject.SetActive(false);
            }
            if (LeftHandLineVisual != null) 
            {
                LeftHandLineVisual.SetActive(false);
            }            
            if (RightHandLineVisual != null) 
            {
                RightHandLineVisual.SetActive(false);
            }
        }

        void ClearJointsUpdatedListeners()
        {
            ClearJointsUpdatedEvent(m_MetaQuestLeftHandTrackingEvents);
            ClearJointsUpdatedEvent(m_MetaQuestRightHandTrackingEvents);
            ClearJointsUpdatedEvent(m_AndroidXRLeftHandTrackingEvents);
            ClearJointsUpdatedEvent(m_AndroidXRRightHandTrackingEvents);
        }

        void ClearJointsUpdatedEvent(XRHandTrackingEvents trackingEvents)
        {
            if (trackingEvents == null) return;

            var fieldInfo = typeof(XRHandTrackingEvents).GetField("m_JointsUpdated", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                // Replace with a new HandUpdatedEvent. The Inspector UI will still show entries, but actual invokes become empty.
                fieldInfo.SetValue(trackingEvents, new HandUpdatedEvent());
            }
        }
    }
}