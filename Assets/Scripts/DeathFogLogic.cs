using UnityEngine;

public class DeathFogLogic : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
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