using UnityEngine;
using UnityEngine.UI; // Necessário para Button
using UnityEngine.EventSystems; // Necessário para IPointerClickHandler

[RequireComponent(typeof(Button))] // Garante que o GameObject tenha um Button
public class ButtonSound : MonoBehaviour, IPointerClickHandler
{
    public AudioClip soundClip; // O clipe de áudio a ser tocado
    [Range(0, 1)] // Cria um slider no Inspector para configurar o volume
    public float volume = 1.0f;

    private Button button;

    void Awake()
    {
    // Cria um AudioSource dinamicamente para evitar problemas de concorrência

    button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("ButtonSound script needs to be attached to a GameObject with a Button component.");
            enabled = false; // Desabilita o script se não houver Button
            return;
        }
    }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (!button.interactable)
    {
      return; // Não toca o som se o botão não estiver interagível
    }

    if (soundClip == null)
    {
      AudioManager.Instance.PlaySoundEffect(1, volume);
      return;
    }

    AudioManager.Instance.PlaySoundEffectClip(soundClip, volume);

  }
}
