using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    UIAudioManager _uiAudioManager;

    UIAudioManager UIAudioManager { get { if (_uiAudioManager == null) { _uiAudioManager = FindObjectOfType<UIAudioManager>(); } return _uiAudioManager; } }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIAudioManager.PlayClickSound();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIAudioManager.PlayHoverSound();
    }
}
