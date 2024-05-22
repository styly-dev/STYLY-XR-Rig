using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Styly.XRRig
{
    /// <summary>
    /// Create ARSession and make it DoNotDestroy
    /// </summary>
    public class ARSessionCreator : MonoBehaviour
    {
        private void Awake()
        {
            CreateARSession();
        }

        void CreateARSession()
        {
            // If ARSession is not found in the scene
            if (FindObjectOfType<ARSession>() == null)
            {
                // Create AR Session GameObject and attach ARSession component
                GameObject arSessionGameObject = new("ARSession");
                ARSession arSession = arSessionGameObject.AddComponent<ARSession>();
                // Make the ARSession GameObject DoNotDestroyOnLoad
                DontDestroyOnLoad(arSessionGameObject);
                // Debug log
                Debug.Log("ARSession is created and DoNotDestroyOnLoad");
            }
        }
    }

}
