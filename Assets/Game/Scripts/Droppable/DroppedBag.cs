using System;
using System.Collections.Generic;
using UnityEngine;

public class DroppedBag : BaseInventory, IInteractable
{
    // TODO:: Create an event that destroyes the dropped bag after a minutes when someone leaves or when is empty

    private void Awake() 
    {
        InitializeSlots();    
    }

    private void OnEnable() 
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].OnItemSwap += HandleSwap;
            Slots[i].OnItemAdd += HandleAddItem;
        }
    }

    private void OnDisable() 
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].OnItemSwap -= HandleSwap;
            Slots[i].OnItemAdd -= HandleAddItem;
        }
    }

    public void HandleAddItem(Guid Id, int dPos)
    {
        // Base add item
        AddItem(Id, dPos);

        // Update UI
        StorageUI.Instance.UpdateUISlots(Slots , true);
    }

    public void HandleSwap(Guid Id, int dPos)
    {
        // Base Swap item
        SwapItem(Id, dPos);

        // Update UI
        StorageUI.Instance.UpdateUISlots(Slots, true);
    }

    public override void RemoveItem(int index)
    {
        base.RemoveItem(index);

        if(IsEmptyBag())
        {
            CloseBag();

            // Has someone interacting with it?
            if(pController == null)
                return;

            if(pController.TryGetComponent<Inventory>(out Inventory inv))
            {
                inv.curDroppedBag = null;

                // TODO:: Improve change to pool
                Destroy(gameObject);
            }
        }
        else
        {
            StorageUI.Instance.UpdateUISlots(Slots , true);
        }
    }

    public override void UpdateSlot(ItemSlot itemSlot, int index)
    {
        base.UpdateSlot(itemSlot, index);

        StorageUI.Instance.UpdateUISlots(Slots , true);
    }
    public override void UpdateSlotQuantity(int index, int amount)
    {
        base.UpdateSlotQuantity(index, amount);

        StorageUI.Instance.UpdateUISlots(Slots , true);
    }

    private void OpenBag()
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
        StorageUI.Instance.Toogle(true, true);

        StorageUI.Instance.UpdateTitle("Dropped Bag");
        
        StorageUI.Instance.UpdateUISlots(Slots, true);
    }

    private void CloseBag()
    {
        // Has someone interacting with chest?
        if(pController == null)
            return;

        // Toggle cursor
        pController.ToggleCursor(false);

        // Toggle interaction
        pController.ToggleInteract();

        // If inventory is not open, open
        UIManager.Instance.ToggleInventoryWindow();

        // Open storage window
        StorageUI.Instance.Toogle();
    }

    public bool AddDroppedItem(ItemData item, int quantity)
    {
        // Get empty slot
        int emptySlotPos = GetEmptySlotPosition();
        if(emptySlotPos == -1)
            return false;

        // Update slot
        UpdateSlotByIndex(item, quantity, emptySlotPos);

        // Check if bag is open
        if(StorageUI.Instance.IsInteractingWithDroppedBag)
        {
            StorageUI.Instance.UpdateUISlots(Slots, true);
        }

        return true;
    }

    private bool IsEmptyBag()
    {
        foreach (ItemSlot slot in Slots)
        {
            if(slot.Item != null)
            {
                return false;
            }
        }
        return true;
    }

    #region Interactable events
    public string GetInteractPrompt()
    {
        return "Open";
    }

    public void OnInteract(PlayerController playerController)
    {
        pController = playerController;

        OpenBag();
    }

    public void OnDesinteract(PlayerController playerController)
    {
        pController = playerController;
        
        CloseBag();
    }
    #endregion
}
