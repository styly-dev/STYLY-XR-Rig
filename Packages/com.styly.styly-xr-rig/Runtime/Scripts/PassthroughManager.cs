using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARFoundation;

#if USE_VIVE
using VIVE.OpenXR;
using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.Passthrough;
#endif

#if USE_PICO
using Unity.XR.OpenXR.Features.PICOSupport;
#endif

namespace Styly.XRRig
{
    public class PassthroughManager : MonoBehaviour
    {
        private enum XRMode { VR, MR }
        private Camera mainCameraOfStylyXrRig;
        private float transitionDuration;
        private Color fadeColor = Color.black;

        private readonly Dictionary<Camera, Image> fadeImages = new();
        private Camera[] targetCameras;

        // --- Public API ---
        private bool passthroughMode;
        public bool PassthroughMode
        {
            get => passthroughMode;
            set
            {
                passthroughMode = value;
                if (value) { SwitchToMR(); } else { SwitchToVR(); }
            }
        }

        public void SwitchToVR(float duration = 1)
        {
            passthroughMode = false;
            StartTransition(XRMode.VR, duration);
            Invoke(nameof(DisablePassthroughAPI), duration);
            Invoke(nameof(DisableARCamera), duration);
        }
        public void SwitchToMR(float duration = 1)
        {
            passthroughMode = true;
            EnableARCamera();
            EnablePassthroughAPI();
            StartTransition(XRMode.MR, duration);
        }

        private void EnableARCamera()
        {
            mainCameraOfStylyXrRig.TryGetComponent<ARCameraManager>(out var arCameraManager);
            arCameraManager.enabled = true;
            mainCameraOfStylyXrRig.TryGetComponent<ARCameraBackground>(out var arCameraBackground);
            arCameraBackground.enabled = true;
        }

        private void DisableARCamera()
        {
            mainCameraOfStylyXrRig.TryGetComponent<ARCameraManager>(out var arCameraManager);
            arCameraManager.enabled = false;
            mainCameraOfStylyXrRig.TryGetComponent<ARCameraBackground>(out var arCameraBackground);
            arCameraBackground.enabled = false;
        }

        void Awake()
        {
            if (!ResolveMainCamera()) return;
            BuildTargetsAndOverlays();
            SetUpCameraForPassthrough();
        }

        public void SetUpCameraForPassthrough()
        {
            // Skip on Vision OS
            if (Utils.IsVisionOS()) { return; }

            // Set Main Camera parameteres
            mainCameraOfStylyXrRig.clearFlags = CameraClearFlags.SolidColor;
            // Set Main Camera Background to black with 0 alpha
            mainCameraOfStylyXrRig.backgroundColor = new Color(0, 0, 0, 0);
            // Set HDR Rendering to false
            mainCameraOfStylyXrRig.allowHDR = false;
            // Add AR Camera Manager
            mainCameraOfStylyXrRig.gameObject.AddComponent<ARCameraManager>();
            // Add AR Camera Background
            mainCameraOfStylyXrRig.gameObject.AddComponent<ARCameraBackground>();
            // Debug log
            Debug.Log("Passthrough is setup");
        }

        // --- Core ---
        private void StartTransition(XRMode next, float duration = 1)
        {
            transitionDuration = duration;
            if (!isActiveAndEnabled) return;

            // Just in case cameras were added at runtime
            if (targetCameras == null || targetCameras.Length == 0) BuildTargetsAndOverlays();

            if (transitionDuration <= 0f)
            {
                ApplyMode(next);
                SetFadeAlpha(0f);
                return;
            }

            StopAllCoroutines();                // Prevent overlapping fades
            StartCoroutine(DoTransition(next)); // Fade → Switch → Fade
        }

        private IEnumerator DoTransition(XRMode next)
        {
            float half = transitionDuration * 0.5f;
            yield return Fade(0f, 1f, half);
            ApplyMode(next);
            yield return Fade(1f, 0f, half);
            SetFadeAlpha(0f);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (duration <= 0f) { SetFadeAlpha(to); yield break; }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                SetFadeAlpha(a);
                yield return null;
            }
            SetFadeAlpha(to);
        }

        private void ApplyMode(XRMode mode)
        {
            foreach (var cam in targetCameras)
            {
                if (!cam) continue;

                if (mode == XRMode.VR)
                {
                    cam.clearFlags = CameraClearFlags.Skybox;
                }
                else // MR
                {
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    var bg = cam.backgroundColor; bg.a = 0f;
                    cam.backgroundColor = bg;
                }
            }
        }

        // --- Setup ---
        private bool ResolveMainCamera()
        {
            // Find Main Camera of STYLY-XR-Rig
            var STYLYXRRig = GameObject.FindFirstObjectByType<Styly.XRRig.StylyXrRig>();
            mainCameraOfStylyXrRig = STYLYXRRig.transform.GetComponentsInChildren<Camera>().FirstOrDefault(camera => camera.CompareTag("MainCamera"));
            return true;
        }

        private void BuildTargetsAndOverlays()
        {
            targetCameras = mainCameraOfStylyXrRig.GetComponentsInChildren<Camera>(true);
            if (targetCameras == null || targetCameras.Length == 0)
                targetCameras = new[] { mainCameraOfStylyXrRig };

            foreach (var cam in targetCameras)
            {
                if (!cam || fadeImages.ContainsKey(cam)) continue;
                fadeImages[cam] = CreateFadeOverlay(cam);
            }
        }

        private Image CreateFadeOverlay(Camera cam)
        {
            var canvasGO = new GameObject($"FadeCanvas_{cam.name}");
            canvasGO.transform.SetParent(cam.transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.sortingOrder = short.MaxValue; // Front-most layer

            canvasGO.AddComponent<CanvasScaler>();

            var imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(canvasGO.transform, false);

            var img = imageGO.AddComponent<Image>();
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var c = fadeColor; c.a = 0f;
            img.color = c;
            img.raycastTarget = false;

            return img;
        }

        private void SetFadeAlpha(float alpha)
        {
            foreach (var img in fadeImages.Values)
            {
                if (!img) continue;
                var c = fadeColor;
                c.a = alpha;
                img.color = c;
            }
        }

        // Device specific code ----------------------------------

#if USE_VIVE
        private VIVE.OpenXR.Passthrough.XrPassthroughHTC activePassthroughID = 0;
        private LayerType currentActiveLayerType = LayerType.Underlay;
#endif

        private void EnablePassthroughAPI()
        {
#if USE_VIVE
            var result = PassthroughAPI.CreatePlanarPassthrough(out activePassthroughID, currentActiveLayerType, OnDestroyPassthroughFeatureSession);
            if (result != XrResult.XR_SUCCESS)
            {
                Debug.LogError($"Failed to create passthrough: {result}");
            }
            SetPassthroughToUnderlay();
#endif

#if USE_PICO
            // Check if OpenXR Passthrough feature is enabled
            if (PassthroughFeature.isExtensionEnable)
            {
                // Create full screen layer + Start is processed internally
                PassthroughFeature.EnableVideoSeeThrough = true; // => createFullScreenLayer() → passthroughStart()
            }
#endif
        }

        private void DisablePassthroughAPI()
        {
#if USE_VIVE
            PassthroughAPI.DestroyPassthrough(activePassthroughID);
            activePassthroughID = 0;
#endif

#if USE_PICO
            if (PassthroughFeature.isExtensionEnable)
            {
                PassthroughFeature.EnableVideoSeeThrough = false; // => passthroughPause()
            }
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

        private void OnApplicationPause(bool pause)
        {
#if USE_PICO
            if (!PassthroughFeature.isExtensionEnable) return;
            if (pause) PassthroughFeature.PassthroughPause();
            else PassthroughFeature.PassthroughStart();
#endif
        }
    }
}