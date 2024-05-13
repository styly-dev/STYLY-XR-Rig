using UnityEngine;

public class DisableOnBuildApp : MonoBehaviour
{
    void Start()
    {
        // Skip if running in the editor
        if (Application.isEditor) { return; }

        // Disable the game object
        DisableGameObject();
    }
    void DisableGameObject()
    {
        gameObject.SetActive(false);
    }

}
