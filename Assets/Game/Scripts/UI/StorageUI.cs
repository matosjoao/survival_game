using System.Collections.Generic;
using UnityEngine;

public class StorageUI : Singleton<StorageUI>
{
    [Header("Properties")]
    [SerializeField] private RectTransform slotsParent;
    [SerializeField] private GameObject window;
    [SerializeField] private ItemSlotUI slotUIPrefab;
    [SerializeField] private MouseFollower slotUIDraggable;
    [SerializeField] private int maxSlotsSize;

    [Header("Items UI")]
    public List<ItemSlotUI> slotsUI;

    private ItemSlotUI currentDraggedItemSlotUI;
    private bool isInteractingWithBag;
    public bool IsInteractingWithBag => isInteractingWithBag;

    private void Start()
    {
        isInteractingWithBag = false;
        slotsUI = new List<ItemSlotUI>();
       
        for(int i = 0; i < maxSlotsSize; i++)
        {
            ItemSlotUI slotUI = Instantiate(slotUIPrefab, Vector3.zero, Quaternion.identity);
            slotUI.transform.SetParent(slotsParent);
            slotUI.transform.localScale = Vector3.one;
            slotUI.gameObject.SetActive(false);

            slotUI.OnItemBeginDrag += HandleItemBeginDrag;
            slotUI.OnItemEndDrag += HandleItemEndDrag;
            slotUI.OnItemDroppedOn += HandleItemDrop;
            
            slotsUI.Add(slotUI);
        }
    }

    private void OnDestroy() 
    {
        for(int i = 0; i < maxSlotsSize; i++)
        {
            if(slotsUI[i] != null)
            {
                slotsUI[i].OnItemBeginDrag -= HandleItemBeginDrag;
                slotsUI[i].OnItemEndDrag -= HandleItemEndDrag;
                slotsUI[i].OnItemDroppedOn -= HandleItemDrop;
            }
        }
    }

    public void Toogle(bool value = false)
    {
        window.SetActive(value);
    }

    public bool IsOpen()
    {
        return window.activeInHierarchy;
    }

    public void UpdateUISlots(ItemSlot[] slots)
    {
        ClearUISlots();

        for (int i = 0; i < slots.Length; i++)
        {
            // Get slot ui from pool
            ItemSlotUI slotUI = GetPooledObject();
            if(slotUI != null)
            {
                if(slots[i].Item != null)
                {
                    slotUI.Set(slots[i]);
                }
                else
                {
                    slotUI.Clear();
                }
                slotUI.SetIndex(i);
                slotUI.gameObject.SetActive(true);
            }
        }
    }

    public void ClearUISlots()
    {
        foreach (ItemSlotUI slotUI in slotsUI)
        {
            slotUI.Clear();
            slotUI.gameObject.SetActive(false);
        }
    }

    public void ClearUISlot(ItemSlot slot, bool active = false)
    {
        foreach (ItemSlotUI slotUI in slotsUI)
        {
            if(slotUI.CurrentItemSlot == slot)
            {
                slotUI.Clear();
                slotUI.gameObject.SetActive(active);
                return;
            }
        }
    }

    public void SetIsInteractingWithBag(bool value = false)
    {
        isInteractingWithBag = value;
    }

    #region ItemSlotUI Pool
    public ItemSlotUI GetPooledObject()
    {
        for(int i = 0; i < maxSlotsSize; i++)
        {
            if(!slotsUI[i].gameObject.activeInHierarchy)
            {
                return slotsUI[i];
            }
        }
        return null;
    }
    #endregion

    #region Slot UI Events
    private void HandleItemDrop(ItemSlotUI itemSlotUI)
    {
        if(slotUIDraggable.IsInventory)
        {
            // Dropped from inventory
            itemSlotUI.AddToStorageFromInventory(slotUIDraggable.UISlot.Index);
        }
        else
        {
            // Check if we are graggin an item
            if(slotUIDraggable.UISlot.CurrentItemSlot == null)
                return;

            // Swipe in Storage
            itemSlotUI.SwapStorage(slotUIDraggable.UISlot.Index);
        }
    }

    private void HandleItemBeginDrag(ItemSlotUI itemSlotUI)
    {   
        // Slot has item
        if(itemSlotUI.CurrentItemSlot == null)
            return;
        
        // Assign current slot index
        currentDraggedItemSlotUI = itemSlotUI;

        // Show draggable slot in mouse position
        slotUIDraggable.Toggle(true);
        slotUIDraggable.SetData(itemSlotUI.CurrentItemSlot, itemSlotUI.Index, false);
    }

    private void HandleItemEndDrag(ItemSlotUI itemSlotUI)
    {   
        slotUIDraggable.Toggle(false);
        currentDraggedItemSlotUI = null;
    }
    #endregion
}
