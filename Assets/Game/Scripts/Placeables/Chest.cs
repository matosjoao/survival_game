using System;
using UnityEngine;

public class Chest : BaseInventory, IInteractable
{
    [Header("Properties")]
    [SerializeField] private string uiTitle;
    
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
            Slots[i].OnItemDrop += HandleDropItem;
        }
    }

    private void OnDisable() 
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].OnItemSwap -= HandleSwap;
            Slots[i].OnItemAdd -= HandleAddItem;
            Slots[i].OnItemDrop -= HandleDropItem;
        }
    }

    private void HandleOnPlayerLeaves(PlayerController playerController)
    {
        if(playerController == pController)
        {
            // Same player that was interaction

            // Close storage window
            StorageUI.Instance.Toogle(false);
        }
    }

    public void HandleAddItem(Guid id, int dPos)
    {
        // Base add item
        AddItem(id, dPos);

        // Update UI
        StorageUI.Instance.UpdateUISlots(Slots);
    }

    public void HandleSwap(Guid id, int dPos)
    {
        // Base Swap item
        SwapItem(id, dPos);

        // Update UI
        StorageUI.Instance.UpdateUISlots(Slots);
    }

    private void HandleDropItem(Guid id)
    {
        // Base Swap item
        DropItem(id);
        
        // Update UI
        StorageUI.Instance.UpdateUISlots(Slots);
    }

    public override void RemoveItem(int index)
    {
        base.RemoveItem(index);

        StorageUI.Instance.UpdateUISlots(Slots);
    }

    public override void UpdateSlot(ItemSlot itemSlot, int index)
    {
        base.UpdateSlot(itemSlot, index);

        StorageUI.Instance.UpdateUISlots(Slots);
    }
    
    public override void UpdateSlotQuantity(int index, int amount)
    {
        base.UpdateSlotQuantity(index, amount);

        StorageUI.Instance.UpdateUISlots(Slots);
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

        // Open storage window
        StorageUI.Instance.Toogle(true);

        StorageUI.Instance.UpdateTitle(uiTitle);

        StorageUI.Instance.UpdateUISlots(Slots);
    }

    private void CloseChest()
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

    public void OnDesinteract(PlayerController playerController)
    {
        pController = playerController;

        CloseChest();
    }
    #endregion
}
