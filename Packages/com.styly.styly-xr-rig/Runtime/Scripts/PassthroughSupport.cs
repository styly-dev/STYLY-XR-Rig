using UnityEngine;
#if USE_VIVE
using VIVE.OpenXR;
using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.Passthrough;
#endif

namespace Styly.XRRig
{
    public class PassthroughSupport : MonoBehaviour
    {
#if USE_VIVE
        private VIVE.OpenXR.Passthrough.XrPassthroughHTC activePassthroughID = 0;
        private LayerType currentActiveLayerType = LayerType.Underlay;
#endif
        private void Start()
        {
            StartPassthrough();
        }
        
        public void ToVR()
        {
            StopPassthrough();
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        }

        public void ToMR()
        {
            StartPassthrough();
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
        
        private void StartPassthrough()
        {
#if USE_VIVE
            var result = PassthroughAPI.CreatePlanarPassthrough(out activePassthroughID, currentActiveLayerType, OnDestroyPassthroughFeatureSession);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError($"Failed to create passthrough: {result}");
            }
            SetPassthroughToUnderlay();
#endif
        }
        
        private void StopPassthrough()
        {
#if USE_VIVE
            PassthroughAPI.DestroyPassthrough(activePassthroughID);
            activePassthroughID = 0;
#endif
        }
        
#if USE_VIVE
        private void OnDestroyPassthroughFeatureSession(VIVE.OpenXR.Passthrough.XrPassthroughHTC passthroughID)
        {
            PassthroughAPI.DestroyPassthrough(passthroughID);
            activePassthroughID = 0;
        }
        
        private void SetPassthroughToUnderlay()
        {
            if (activePassthroughID == 0){ return;}
            PassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Underlay);
            currentActiveLayerType = LayerType.Underlay;
        }
#endif
    }
}