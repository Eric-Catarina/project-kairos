// Local: Assets/Scripts/WinLogic.cs

using UnityEngine;

public class WinLogic : MonoBehaviour
{
    private bool _levelFinished = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (_levelFinished) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            _levelFinished = true;
            
            // O jogador não será mais destruído
            
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.EndLevelTimer();
            }
            
            Time.timeScale = 0f;
            InputStateManager.Instance.SwitchState(InputState.PostGame);
        }
    }
}