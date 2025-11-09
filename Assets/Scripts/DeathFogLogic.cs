using UnityEngine;

public class DeathFogLogic : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Novo: Incrementar contador de mortes
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.IncrementDeathCount();
                Debug.Log("DeathFogLogic: Death count incremented."); 
            }

            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.SoftResetToCheckpoint();
            }
            else
            {
                SceneManagerLogic.Instance.RestartScene();
            }
        }
    }
}