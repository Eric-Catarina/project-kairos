using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;

public class BackButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if(GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.HandlePauseRequest();
        }

    }


}
