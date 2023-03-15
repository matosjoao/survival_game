using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StorageUI : Singleton<StorageUI>
{
    [Header("Properties")]
    [SerializeField] private RectTransform slotsParent;
    [SerializeField] private GameObject window;
    [SerializeField] private ItemSlotUI slotUIPrefab;
    [SerializeField] private MouseFollower slotUIDraggable;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private int maxSlotsSize;

    [Header("Items UI")]
    private List<ItemSlotUI> slotsUI;

    private bool isInteractingWithDroppedBag = false;
    public bool IsInteractingWithDroppedBag => isInteractingWithDroppedBag;

    private void Awake() 
    {
        slotsUI = new List<ItemSlotUI>();

        for(int i = 0; i < maxSlotsSize; i++)
        {
            ItemSlotUI slotUI = Instantiate(slotUIPrefab, Vector3.zero, Quaternion.identity);
            slotUI.transform.SetParent(slotsParent);
            slotUI.transform.localScale = Vector3.one;
            slotUI.gameObject.SetActive(false);
            
            slotsUI.Add(slotUI);
        }
    }

    private void OnEnable() 
    {
        if(slotsUI.Count == 0)
            return;

        for(int i = 0; i < maxSlotsSize; i++)
        {
            if(slotsUI[i] != null)
            {
                slotsUI[i].OnItemBeginDrag += HandleItemBeginDrag;
                slotsUI[i].OnItemEndDrag += HandleItemEndDrag;
                slotsUI[i].OnItemDroppedOn += HandleItemDrop;
            }
        }
    }

    private void OnDisable() 
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

    public void Toogle(bool value = false, bool isDroppedBag = false)
    {
        window.SetActive(value);

        isInteractingWithDroppedBag = isDroppedBag;
    }

    public bool IsOpen()
    {
        return window.activeInHierarchy;
    }

    public void UpdateUISlots(ItemSlot[] slots, bool isBag = false)
    {
        ClearUISlots();

        for (int i = 0; i < slots.Length; i++)
        {
            // Get slot ui from pool
            ItemSlotUI slotUI = GetPooledObject();
            if(slotUI != null)
            {
                slotUI.Set(slots[i]);
                slotUI.SetIndex(i);
                
                if(isBag)
                {
                    if(slots[i].Item != null)
                    {
                        slotUI.gameObject.SetActive(true);
                    }
                }
                else
                {
                    slotUI.gameObject.SetActive(true);
                }
            }
        }
    }

    public void UpdateTitle(string value)
    {
        title.text = value;
    }

    private void ClearUISlots()
    {
        foreach (ItemSlotUI slotUI in slotsUI)
        {
            slotUI.Clear();
            slotUI.gameObject.SetActive(false);
        }
    }
    #region ItemSlotUI Pool
    private ItemSlotUI GetPooledObject()
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
            itemSlotUI.CurrentItemSlot.AddItem(slotUIDraggable.UISlot.Index);
        }
        else
        {
            // Check if we are graggin an item
            if(slotUIDraggable.UISlot.CurrentItemSlot == null)
                return;

            // Swipe in Storage
            itemSlotUI.CurrentItemSlot.SwapItems(slotUIDraggable.UISlot.Index);
        }
    }

    private void HandleItemBeginDrag(ItemSlotUI itemSlotUI)
    {   
        // Slot has item
        if(itemSlotUI.CurrentItemSlot == null)
            return;

        // Show draggable slot in mouse position
        slotUIDraggable.Toggle(true);
        slotUIDraggable.SetData(itemSlotUI.CurrentItemSlot, itemSlotUI.Index, false);
    }

    private void HandleItemEndDrag(ItemSlotUI itemSlotUI)
    {   
        slotUIDraggable.Toggle(false);
    }
    #endregion
}
