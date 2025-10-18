using UnityEngine;

public class JumpPadAudioTrigger : MonoBehaviour
{
    private JumpPad pad;
    private PlayerAudioHandler playerAudio;

    public void Setup(JumpPad pad, PlayerAudioHandler handler)
    {
        this.pad = pad;
        this.playerAudio = handler;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAudio.PlayJumpPadAudio();
        }
    }
}
