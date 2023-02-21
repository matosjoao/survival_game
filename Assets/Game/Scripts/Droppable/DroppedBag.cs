using System.Collections.Generic;
using UnityEngine;

public class DroppedBag : MonoBehaviour, IInteractable
{
    [Header("Properties")]
    [SerializeField] private int maxSlotsSize;

    [Header("Items")]
    public List<ItemSlot> slots = new List<ItemSlot>();

    private void Start() 
    {
        // Has items in slots already
        if(slots.Count > 0)
        {
            foreach (ItemSlot slot in slots)
            {
                // Subscribe to events
                //slot.OnItemSlotDraggedToInventory += HandleItemSlotDraggedToInventoryFromBag;
                //slot.OnItemSwapBag += HandleOnItemSwapBag;
            }
        }
    }

    private void OnDestroy() 
    {
        // Has items in slots 
        if(slots.Count > 0)
        {
            foreach (ItemSlot slot in slots)
            {
                // Unsubscribe to events
                //slot.OnItemSlotDraggedToInventory -= HandleItemSlotDraggedToInventoryFromBag;
                //slot.OnItemSwapBag -= HandleOnItemSwapBag;
            }
        }
    }

    public void AddItem(ItemData item, int quantity)
    {
        // Add item bag

        // Fiquei aqui!!!
        // Can stack
        /* if(itemSlot.item.canStack)
        {
            ItemSlot slotToStackTo = GetItemStack(itemSlot.item);
            if(slotToStackTo != null)
            {
                // Verify quantities

                // slotToStackTo.quantity++;

                // Update UI Slots
                // UIManager.Instance.UpdateInventorySlots(slots);
                return;
            }
        } */

        // Reached max slots
        if(slots.Count == maxSlotsSize)
            return;

        // Create Slot
        ItemSlot newSlot = new ItemSlot();
        //newSlot.item = item;
        //newSlot.quantity = quantity;

        // Subscribe to events
        //newSlot.OnItemSlotDraggedToInventory += HandleItemSlotDraggedToInventoryFromBag;
        //newSlot.OnItemSwapBag += HandleOnItemSwapBag;

        // Add to slots
        slots.Add(newSlot);

        // If window open, update ui slots
        if(StorageUI.Instance.IsOpen() && StorageUI.Instance.IsInteractingWithBag)
        {
            StorageUI.Instance.UpdateUISlots(slots.ToArray());
        }
    }

    public bool CanAddItem()
    {
        // Can stack


        // Reached max slots
        if(slots.Count == maxSlotsSize)
            return false;

        return true;
    }

    private ItemSlot GetItemStack(ItemData item)
    {
        // Search for a slot of the same type
        for (int i = 0; i < slots.Count; i++)
        {
            if(slots[i].Item == item && slots[i].Quantity < item.maxStackAmount)
            {
                return slots[i];
            }
        }

        return null;
    }

    private void OpenBag()
    {
        // Toggle cursor
        //UIManager.Instance.ToggleCursor(true);

        // Toggle interaction
        //UIManager.Instance.ToggleInteract(false);

        // If inventory is not open, open
        if(!UIManager.Instance.IsInventoryOpen())
        {
            UIManager.Instance.ToggleInventoryWindow(true);
        }

        // Open bag window
        StorageUI.Instance.Toogle(true);

        StorageUI.Instance.UpdateUISlots(slots.ToArray());

        StorageUI.Instance.SetIsInteractingWithBag(true);
    }

    private void HandleItemSlotDraggedToInventoryFromBag(ItemSlot itemSlot)
    {
        Debug.Log("Entour Bag");
        foreach (ItemSlot slot in slots)
        {
            if(slot == itemSlot)
            {   
                // Unsubscribe to events
                //slot.OnItemSlotDraggedToInventory -= HandleItemSlotDraggedToInventoryFromBag;
                //slot.OnItemSwapBag -= HandleOnItemSwapBag;

                // Remove slot
                slots.Remove(slot);

                // Update UI
                StorageUI.Instance.ClearUISlot(slot);
                return;
            }
        }
    }

    private void HandleOnItemSwapBag(ItemSlot swapItem, ItemSlot toSwapItem)
    {
        // Get positions
        int swapItemPos = slots.IndexOf(swapItem);
        int toSwapItemPos = slots.IndexOf(toSwapItem);

        // Swap items
        slots[swapItemPos] = toSwapItem;
        slots[toSwapItemPos] = swapItem;

        // Update UI
        StorageUI.Instance.UpdateUISlots(slots.ToArray());
    }

    #region Interactable events
    public string GetInteractPrompt()
    {
        return "Open";
    }

    public void OnInteract(PlayerController playerController)
    {
        OpenBag();
    }
    #endregion
}
