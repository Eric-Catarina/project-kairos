using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathFogLogic : MonoBehaviour
{
    [SerializeField] SceneManagerLogic sceneManager;
    void Start()
    {
        if (sceneManager == null)
        {
            sceneManager = FindObjectOfType<SceneManagerLogic>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
            sceneManager.RestartScene();
        }
    }
}
