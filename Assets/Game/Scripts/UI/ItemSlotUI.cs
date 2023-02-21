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

    public void AddToStorageFromInventory(int invIndex)
    {
        EventBus.Instance.Publish("AddToStorageFromInventory", new SwapItemsModel(Index, invIndex));
    }

    public void SwapStorage(int stoIndex)
    {
        EventBus.Instance.Publish("SwapStorage", new SwapItemsModel(Index, stoIndex));

        //CurrentItemSlot.ItemSlotSwappedInBag(swapItem);
    }

    public void DropToBag(ItemSlot swapItem)
    {
        if(CurrentItemSlot == null)
            return;

        //CurrentItemSlot.ItemSlotDropInBag(swapItem);
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

    public void OnDrag(PointerEventData eventData) {}
    #endregion
}
