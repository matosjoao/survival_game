using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Dropzone : MonoBehaviour, IDropHandler
{
    [HideInInspector] public event Action OnItemDroppedOnDropZone;

    public void OnDrop(PointerEventData eventData)
    {
        OnItemDroppedOnDropZone?.Invoke();
    }
}
