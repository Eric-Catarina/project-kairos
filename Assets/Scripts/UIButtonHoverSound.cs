using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{

    public string hoverSoundName = "UIHover";

    public bool ignoreTimeScale = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.instance == null || string.IsNullOrEmpty(hoverSoundName))
        {
            Debug.LogWarning("AudioManager.instance não encontrado ou hoverSoundName não definido.");
            return;
        }

        if (ignoreTimeScale)
        {
            AudioManager.instance.PlayUnscaledSFX(hoverSoundName);
        }
        else
        {
            AudioManager.instance.PlaySFX(hoverSoundName);
        }
    }
}