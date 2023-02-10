using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ItemsSlotsUI : Singleton<ItemsSlotsUI>
{
    [Header("Properties")]
    [SerializeField] private RectTransform slotsParent;
    [SerializeField] private GameObject window;
    [SerializeField] private ItemSlotUI slotUIPrefab;
    [SerializeField] private MouseFollower slotUIDraggable;

    [Header("Items UI")]
    private ObjectPool<ItemSlotUI> poolSlotsUI ;
    private ItemSlotUI currentDraggedItemSlotUI;

    private void Awake() 
    {
        poolSlotsUI = new ObjectPool<ItemSlotUI>(CreateItemSlotUI, OnTakeItemSlotUIFromPoll, OnReturnItemSlotUIToPoll);
    }

    public void Toogle(bool value = false)
    {
        window.SetActive(value);
    }

    public bool IsBagOpen()
    {
        return window.activeInHierarchy;
    }

    public void UpdateUISlots(List<ItemSlot> slots)
    {
        ClearUISlots();

        for (int i = 0; i < slots.Count; i++)
        {
            // Get slot ui from pool
            ItemSlotUI slotUI = poolSlotsUI.Get();
            slotUI.Set(slots[i]);
        }
    }

    public void ClearUISlots()
    {
        foreach(Transform slot in slotsParent)
        {
            // Clear slot
            ItemSlotUI slotUI = slot.GetComponent<ItemSlotUI>();
            slotUI.Clear();
            
            if(slot.gameObject.activeInHierarchy)
            {
                // Release slot ui to pool
                poolSlotsUI.Release(slotUI);
            }
        }
    }

    public void ClearUISlot(ItemSlot slot)
    {
        foreach(Transform slotTransform in slotsParent)
        {
            ItemSlotUI slotUI = slotTransform.GetComponent<ItemSlotUI>();
            if(slotUI.CurrentItemSlot == slot)
            {
                slotUI.Clear();
                poolSlotsUI.Release(slotUI);
                return;
            }
        }
    }

    #region ItemSlotUI Pool
    private ItemSlotUI CreateItemSlotUI()
    {
        ItemSlotUI slotUI = Instantiate(slotUIPrefab, Vector3.zero, Quaternion.identity);
        slotUI.transform.SetParent(slotsParent);
        slotUI.transform.localScale = Vector3.one;

        return slotUI;
    }

    private void OnTakeItemSlotUIFromPoll(ItemSlotUI slotUI)
    {
        slotUI.gameObject.SetActive(true);

        // Subscrive to events
        slotUI.OnItemBeginDrag += HandleItemBeginDrag;
        slotUI.OnItemEndDrag += HandleItemEndDrag;
        slotUI.OnItemDroppedOn += HandleItemDrop;
    }

    private void OnReturnItemSlotUIToPoll(ItemSlotUI slotUI)
    {
        slotUI.gameObject.SetActive(false);
        slotUI.Clear();

        // UnSubscrive to events
        slotUI.OnItemBeginDrag -= HandleItemBeginDrag;
        slotUI.OnItemEndDrag -= HandleItemEndDrag;
        slotUI.OnItemDroppedOn -= HandleItemDrop;
    }
    #endregion

    #region Slot UI Events
    private void HandleItemDrop(ItemSlotUI itemSlotUI)
    {
        Debug.Log("Drop in Bag");
        // Swipe in Bag
        
        // TODO:: Try to solve this without singleton
        // Inventory.Instance.SwapItems(itemSlotUI.Index, currentDraggedItemIndex); 
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
        slotUIDraggable.SetData(itemSlotUI.CurrentItemSlot, false);
    }

    private void HandleItemEndDrag(ItemSlotUI itemSlotUI)
    {   
        slotUIDraggable.Toggle(false);
        currentDraggedItemSlotUI = null;
    }
    #endregion
}
