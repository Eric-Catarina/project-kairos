using UnityEngine;

public class DeathFogLogic : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.SoftResetToCheckpoint();
            }
            else
            {
                ScoreManager.Instance?.IncrementDeathCount();
                SceneManagerLogic.Instance.RestartScene();
            }
        }
    }
}