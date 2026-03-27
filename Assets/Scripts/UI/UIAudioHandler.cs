using UnityEngine;
using UnityEngine.EventSystems;
using FMODUnity;
using GamePlay;

public class UIAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        RuntimeManager.PlayOneShot(FMODEvents.SFX.UI.Hover);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RuntimeManager.PlayOneShot(FMODEvents.SFX.UI.Click);
    }
}