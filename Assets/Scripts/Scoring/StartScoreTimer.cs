using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class StartScoreTimer : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ScoreManager.Instance.StartLevelTimer();
        }
    }

}
