using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

namespace Styly.XRRig
{
    /// <summary>
    /// Disable the hand-tracking interaction ray by setting the cast distance of the NearFarInteractor's CurveInteractionCaster to zero.
    /// Attach this component to a GameObject and assign the left and right CurveInteractionCaster references, then enable it.
    /// </summary>
    public class DisableNearFarInteractorCurveInteractionCaster : MonoBehaviour
    {
        [SerializeField]
        CurveInteractionCaster leftNearFarInteractorCurveInteractionCaster;
        [SerializeField]
        CurveInteractionCaster rightNearFarInteractorCurveInteractionCaster;

        void Start()
        {
            leftNearFarInteractorCurveInteractionCaster.castDistance = 0f;
            rightNearFarInteractorCurveInteractionCaster.castDistance = 0f;
        }
    }
}
