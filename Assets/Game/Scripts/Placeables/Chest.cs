using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("Properties")]
    [SerializeField] private int maxSlotsSize;

    [Header("Items")]
    public ItemSlot[] slots;

    private PlayerController pController;

    private void OnEnable() 
    {
        EventBus.Instance.Subscribe("AddToStorageFromInventory", AddItem);
        EventBus.Instance.Subscribe("SwapStorage", SwapItem);
    }

    private void OnDisable() 
    {
        EventBus.Instance.Unsubscribe("AddToStorageFromInventory", AddItem);
        EventBus.Instance.Unsubscribe("SwapStorage", SwapItem);
    }

    private void Start() 
    {
        // Initialize slots
        slots = new ItemSlot[maxSlotsSize];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemSlot();
        }
    }

    private void OpenChest()
    {
        // Has someone interacting with chest?
        if(pController == null)
            return;

        // Toggle cursor
        pController.ToggleCursor(true);

        // Toggle interaction
        pController.ToggleInteract(true);

        // If inventory is not open, open
        if(!UIManager.Instance.IsInventoryOpen())
        {
            UIManager.Instance.ToggleInventoryWindow(true);
        }

        // Open bag window
        StorageUI.Instance.Toogle(true);

        StorageUI.Instance.UpdateUISlots(slots);
    }

    private void AddItem(object data)
    {
        // Has someone interacting with chest?
        if(pController == null)
            return;
        
        // Get player inventory
        Inventory pInventory = pController.GetComponent<Inventory>();
        if(pInventory == null)
            return;

        // Get data
        SwapItemsModel eventData = data as SwapItemsModel;
        int chestPos = eventData.targetPosition;
        int invPos = eventData.draggablePosition;

        // Validate inventory item data
        ItemSlot itemSlot = pInventory.GetItemInSlot(invPos);
        if(itemSlot == null)
            return;

        // Has item in slot
        if(slots[chestPos].Item != null)
        {
            // Is the same type and can stack and as room left?
            if(slots[chestPos].Item == itemSlot.Item && slots[chestPos].Item.canStack && slots[chestPos].RoomLeftInStack(itemSlot.Quantity, out int roomLeftQuantity ))
            {
                // Calculate quantity to add
                int quantityToAdd = Mathf.Min(roomLeftQuantity, itemSlot.Quantity);

                // Exchange quantities
                slots[chestPos].AddQuantity(quantityToAdd);

                // Update quantity in inventory
                pInventory.UpdateSlotQuantity(invPos, quantityToAdd);
            }
            else
            {
                ItemSlot item1 = new ItemSlot(slots[chestPos].Item, slots[chestPos].Quantity);

                // Assign item to storage slot
                slots[chestPos].UpdateSlot(itemSlot.Item, itemSlot.Quantity);

                // Swap between inventory and storage
                pInventory.UpdateSlot(item1, invPos);
            }
        }
        else
        {
            // Assign item to storage slot
            slots[chestPos].UpdateSlot(itemSlot.Item, itemSlot.Quantity);

            // Remove item from inventory
            pInventory.RemoveItem(invPos); 
        }

        // Update UI
        StorageUI.Instance.UpdateUISlots(slots);
    }

    private void SwapItem(object data)
    {
        SwapItemsModel eventData = data as SwapItemsModel;

        int tPos = eventData.targetPosition;
        int dPos = eventData.draggablePosition;

        // Swap slots
        ItemSlot item1 = slots[dPos];
        slots[dPos] = slots[tPos];
        slots[tPos] = item1;

        // Update UI
        StorageUI.Instance.UpdateUISlots(slots);
    }

    #region Interactable events
    public string GetInteractPrompt()
    {
        return "Open";
    }

    public void OnInteract(PlayerController playerController)
    {
        pController = playerController;

        OpenChest();
    }
    #endregion
}
