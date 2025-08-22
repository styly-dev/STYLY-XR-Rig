using UnityEngine;
using UnityEngine.InputSystem;

namespace Styly.XRRig
{
    public class XriBindingsTweaker : MonoBehaviour
    {
        [SerializeField] private InputActionAsset xriActions;

        void Awake()
        {
            var leftSelect = Find("XRI Left Interaction/Select");
            if (leftSelect != null)
            {
                leftSelect.AddBinding("<XREALHandTracking>{LeftHand}/indexPressed")
                    .WithInteraction("Press(pressPoint=0.5)");
            }

            var rightSelect = Find("XRI Right Interaction/Select");
            if (rightSelect != null)
            {
                rightSelect.AddBinding("<XREALHandTracking>{RightHand}/indexPressed")
                    .WithInteraction("Press(pressPoint=0.5)");
            }

            xriActions.Enable();
        }

        private InputAction Find(params string[] paths)
        {
            foreach (var path in paths)
            {
                var action = xriActions.FindAction(path, false);
                if (action != null) return action;
            }

            return null;
        }
    }
}