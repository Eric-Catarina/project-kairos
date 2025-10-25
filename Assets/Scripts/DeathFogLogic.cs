// Local: Assets/Scripts/DeathFogLogic.cs

using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class DeathFogLogic : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
            SceneManagerLogic.Instance.RestartScene();
        }
    }
}