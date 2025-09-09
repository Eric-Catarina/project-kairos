using UnityEngine;

public class TemporaryBGmusic : MonoBehaviour
{
    float cooldown = 1.5f;
    bool played = false;

    private void Update()
    {
        if (cooldown > 0)
        {
            cooldown--;
            return;
        }
        if (cooldown < 0 && !played) { AudioManager.instance.PlayMusic("Level 1");  played = true; }
    }
}

