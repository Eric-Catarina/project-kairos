using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonClickSound : MonoBehaviour, IPointerClickHandler
{

    public string clickSoundName = "UISoundClick";

    public bool ignoreTimeScale = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.instance == null || string.IsNullOrEmpty(clickSoundName))
        {
            Debug.LogWarning("AudioManager.instance não encontrado ou clickSoundName não definido.");
            return;
        }

        if (ignoreTimeScale)
        {
            AudioManager.instance.PlayUnscaledSFX(clickSoundName);
        }
        else
        {
            AudioManager.instance.PlaySFX(clickSoundName);
        }

        Debug.Log($"Chamou função click. Unscaled: {ignoreTimeScale}");
    }
}