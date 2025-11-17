using UnityEngine;

public class WinLogic : MonoBehaviour
{
    private bool _levelFinished = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (_levelFinished || !collision.gameObject.CompareTag("Player")) return;

        _levelFinished = true;
        
        GameFlowManager.Instance.CompleteLevel(false);
    }
}