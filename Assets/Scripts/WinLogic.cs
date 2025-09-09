using UnityEngine;

public class WinLogic : MonoBehaviour
{
    [SerializeField] SceneManagerLogic sceneManager;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
            sceneManager.LoadNextScene();
        }
    }
}
