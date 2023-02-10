using System.Collections.Generic;
using UnityEngine;

public class DroppedBag : MonoBehaviour, IInteractable
{
    [Header("Properties")]
    [SerializeField] private int maxSlotsSize;

    [Header("Items")]
    public List<ItemSlot> slots = new List<ItemSlot>();

    public void AddItem(ItemData item, int quantity)
    {
        // Add item bag
        Debug.Log("Adicionar ao saco");

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
        newSlot.item = item;
        newSlot.quantity = quantity;
        newSlot.OnItemSlotDraggedToInventory += HandleItemSlotDraggedToInventoryFromBag;

        // Add to slots
        slots.Add(newSlot);

        // If window open, update ui slots
        if(ItemsSlotsUI.Instance.IsBagOpen())
        {
            ItemsSlotsUI.Instance.UpdateUISlots(slots);
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
            if(slots[i].item == item && slots[i].quantity < item.maxStackAmount)
            {
                return slots[i];
            }
        }

        return null;
    }

    private void OpenBag()
    {
        // Toggle cursor
        UIManager.Instance.ToggleCursor(true);

        // Toggle interaction
        UIManager.Instance.ToggleInteract(false);

        // If inventory is not open, open
        if(!UIManager.Instance.IsInventoryOpen())
        {
            UIManager.Instance.ToggleInventoryWindow(true);
        }

        // Open bag window
        ItemsSlotsUI.Instance.Toogle(true);

        ItemsSlotsUI.Instance.UpdateUISlots(slots);
    }

    private void HandleItemSlotDraggedToInventoryFromBag(ItemSlot itemSlot)
    {
        foreach (ItemSlot slot in slots)
        {
            if(slot == itemSlot)
            {
                slot.OnItemSlotDraggedToInventory -= HandleItemSlotDraggedToInventoryFromBag;
                slots.Remove(slot);

                ItemsSlotsUI.Instance.ClearUISlot(slot);
                return;
            }
        }
    }

    #region Interactable events
    public string GetInteractPrompt()
    {
        return "Open";
    }

    public void OnInteract()
    {
        OpenBag();
    }
    #endregion
}
