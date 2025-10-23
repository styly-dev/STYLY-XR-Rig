using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

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
