using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler
{
    [Header("Properties")]
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI quantityText;

    private Outline outline;
    private RectTransform rectTransform;

    [HideInInspector] public bool Equipped { get; private set;}
    [HideInInspector] public int Index { get; private set;}
    [HideInInspector] public ItemSlot CurrentItemSlot { get; private set;}
    [HideInInspector] public event Action<ItemSlotUI> OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag, OnItemRightMouseClick;

    private void Awake() 
    {
        outline = GetComponent<Outline>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable() 
    {
        outline.enabled = Equipped;
    }

    public void Set(ItemSlot slot)
    {
        CurrentItemSlot = slot;

        icon.gameObject.SetActive(true);
        icon.sprite = slot.item.icon;

        quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : string.Empty;

        if(outline != null)
        {
            outline.enabled = Equipped;
        }
    }

    public void Clear()
    {
        CurrentItemSlot = null;
        
        icon.gameObject.SetActive(false);
        quantityText.text = string.Empty;

        icon.gameObject.SetActive(false);
        icon.sprite = null;
    }

    public void SetEquipped(bool value)
    {
        Equipped = value;
    }

    public void SetIndex(int value)
    {
        Index = value;
    }

    #region Events
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            OnItemRightMouseClick?.Invoke(this);
        }
    }

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

    public void OnDrag(PointerEventData eventData) {}
    #endregion
    
}
