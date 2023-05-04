using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler
{
    [Header("Properties")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI quantityText;

    [HideInInspector] public int Index { get; private set;}
    [HideInInspector] public ItemSlot CurrentItemSlot { get; private set;}
    [HideInInspector] public event Action<ItemSlotUI> OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag;
    
    public void Set(ItemSlot slot)
    {
        // Set current slot
        CurrentItemSlot = slot;

        if(slot.Item == null)
            return;

        // Set icon
        icon.gameObject.SetActive(true);
        icon.sprite = slot.Item.icon;

        // Set quantity
        quantityText.text = slot.Quantity > 1 ? slot.Quantity.ToString() : string.Empty;
    }

    public void Clear()
    {
        CurrentItemSlot = null;
        
        icon.gameObject.SetActive(false);
        icon.sprite = null;
        quantityText.text = string.Empty;
    }

    public void SetIndex(int value)
    {
        Index = value;
    }

    #region Events
    public void OnBeginDrag(PointerEventData eventData)
    {
        OnItemBeginDrag?.Invoke(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnItemEndDrag?.Invoke(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnItemDroppedOn?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData) { }
    #endregion
}
