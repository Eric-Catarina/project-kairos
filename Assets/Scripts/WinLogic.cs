// Local: Assets/Scripts/WinLogic.cs

using UnityEngine;

public class WinLogic : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.EndLevelTimer();
            }
            
            Time.timeScale = 0f;
            InputStateManager.Instance.SwitchState(InputState.UI);
        }
    }
}