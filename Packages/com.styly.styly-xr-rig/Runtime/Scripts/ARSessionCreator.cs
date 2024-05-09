using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Styly.XRRig
{
    /// <summary>
    /// Create ARSession and make it DoNotDestroy
    /// </summary>
    public class ARSessionCreator : MonoBehaviour
    {
        private void Start()
        {
            // If ARSession does not exist, create ARSession
            if (!IsARSessionExist())
            {
                CreateARSession();
            }
        }

        bool IsARSessionExist()
        {
            return FindObjectOfType<ARSession>() != null;
        }

        void CreateARSession()
        {
            // Create AR Session GameObject and attach ARSession component
            GameObject arSessionGameObject = new("ARSession");
            ARSession arSession = arSessionGameObject.AddComponent<ARSession>();
            // Make the ARSession GameObject DoNotDestroyOnLoad
            DontDestroyOnLoad(arSessionGameObject);
        }
    }

}
