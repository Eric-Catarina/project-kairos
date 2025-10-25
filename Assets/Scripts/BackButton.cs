using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;

public class BackButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if(UIManager.Instance != null)
        {
            UIManager.Instance.CloseSettingsPanel();
        }
    }


}
