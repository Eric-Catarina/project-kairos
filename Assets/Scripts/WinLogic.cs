using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class WinLogic : MonoBehaviour
{
    [SerializeField] SceneManagerLogic sceneManager;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Player"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.EndLevelTimer();
                
            }
            Destroy(collision.gameObject);
            sceneManager.LoadNextScene();
        }
    }
}
