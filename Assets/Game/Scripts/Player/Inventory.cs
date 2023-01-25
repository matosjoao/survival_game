using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : Singleton<Inventory>
{
    [Header("Components")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private PlayerNeeds playerNeeds;
    [SerializeField] private EquipManager equipManager;

    [Header("Properties")]
    [SerializeField] private Transform dropPosition;
 
    [Header("Selected Item")]
    private ItemSlot selectedItem;
    private int selectedItemIndex;
    
    [Header("Items")]
    private ItemSlot[] slots;

    [Header("Equip Item")]
    private int curEquipIndex;

    private void OnEnable() 
    {
        // Subscribe to events
        inputReader.InventoryEvent += OnInventory;
    }

    private void OnDisable() 
    {
        // Unsubscribe to events
        inputReader.InventoryEvent -= OnInventory;
    }

    private void Start() 
    {
        // Close inventory window
        UIManager.Instance.ToggleInventoryWindow();
        
        // Initialize Slots
        int slotsLength = UIManager.Instance.GetInventorySize();
        slots = new ItemSlot[slotsLength];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemSlot();
        }
        UIManager.Instance.InitializeUISlots(slots);

        // Clear inventory window
        ClearSelectedItemWindow();
    }

    private void OnInventory()
    {
        // Called onClick Tab to inventory

        if(UIManager.Instance.IsInventoryOpen())
        {
            UIManager.Instance.ToggleInventoryWindow();
            //onCloseInventory.Invoke();
            UIManager.Instance.ToggleCursor(false);
        }
        else
        {
            UIManager.Instance.ToggleCursor(true);
            UIManager.Instance.ToggleInventoryWindow(true);
            //onOpenInventory.Invoke();
            ClearSelectedItemWindow();
        }
    }

    public void AddItem(ItemData item)
    {
        // Add item to inventory

        // Can stack
        if(item.canStack)
        {
            ItemSlot slotToStackTo = GetItemStack(item);
            if(slotToStackTo != null)
            {
                slotToStackTo.quantity++;

                // Update UI Slots
                UIManager.Instance.UpdateInventorySlots(slots);
                return;
            }
        }

        // Has empty slot
        ItemSlot emptySlot = GetEmptySlot();
        if(emptySlot != null)
        {
            emptySlot.item = item;
            emptySlot.quantity = 1;

            // Update UI Slots
            UIManager.Instance.UpdateInventorySlots(slots);
            return;
        }

        // TODO:: Improve maybe dont get it if can't stack and no empty slot
        ThrowItem(item);
    }

    private void ThrowItem(ItemData item)
    {
        Instantiate(item.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360.0f));
    }

    private ItemSlot GetItemStack(ItemData item)
    {
        // Search for a slot of the same type
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item == item && slots[i].quantity < item.maxStackAmount)
            {
                return slots[i];
            }
        }

        return null;
    }

    private ItemSlot GetEmptySlot()
    {
        // Search for a empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item == null)
            {
                return slots[i];
            }
        }

        return null;
    }

    public void SelectItem(int index)
    {
        // When onclick from UISlot select item
        if(slots[index].item == null)
        {
            return;
        }

        selectedItem = slots[index];
        selectedItemIndex = index;

        // Update buttons
        UIManager.Instance.ToggleInventoryButtons(selectedItem.item.type, index);
    }

    private void ClearSelectedItemWindow()
    {
        // Clear selected item
        selectedItem = null;
        selectedItemIndex = -1;

        // Disable buttons
        UIManager.Instance.DisableInventoryButtons();
    }

    public void OnUseButton()
    {
        // Onclick use button
        if(selectedItem.item.type == ItemType.Consumable)
        {
            for (int i = 0; i < selectedItem.item.consumables.Length; i++)
            {
                switch (selectedItem.item.consumables[i].type)
                {
                    case ConsumableType.Health: 
                        playerNeeds.Heal(selectedItem.item.consumables[i].value);
                        break;
                    
                    case ConsumableType.Hunger: 
                        playerNeeds.Eat(selectedItem.item.consumables[i].value);
                        break;
                    
                    case ConsumableType.Thirst: 
                        playerNeeds.Drink(selectedItem.item.consumables[i].value);
                        break;

                    default:
                        return;
                }
            }

            RemoveSelectedItem();
        }
    }

    public void OnEquipButton()
    {
        // Onclick equip button

        // If has another equipped, unEquip
        if(UIManager.Instance.IsEquipped(curEquipIndex))
        {
            UnEquip(curEquipIndex);
        }

        // Update ui slot to equipped
        UIManager.Instance.ToggleEquipUISlot(selectedItemIndex, true);

        // Assign current equip index
        curEquipIndex = selectedItemIndex;

        // Equip item
        equipManager.EquipNewItem(selectedItem.item);

        // Update ui slots
        UIManager.Instance.UpdateInventorySlots(slots);

        // Set selected item
        SelectItem(selectedItemIndex);
    }

    private void UnEquip(int index)
    {
        // Update ui slot to equipped
        UIManager.Instance.ToggleEquipUISlot(index, false);

        // UnEquip item
        equipManager.UnEquip();

        // Update ui slots
        UIManager.Instance.UpdateInventorySlots(slots);

        // Set selected item
        if(selectedItemIndex == index)
        {
            SelectItem(index);
        }
    }

    public void OnUnEquipButton()
    {
        UnEquip(selectedItemIndex);
    }

    public void OnDropButton()
    {
        ThrowItem(selectedItem.item);
        RemoveSelectedItem();
    }

    private void RemoveSelectedItem()
    {
        selectedItem.quantity--;
        if(selectedItem.quantity == 0)
        {   
            // TODO:: Improve
            if(UIManager.Instance.IsEquipped(selectedItemIndex))
            {
                UnEquip(selectedItemIndex);
            }

            selectedItem.item = null;
            ClearSelectedItemWindow();
        }

        // Update UI Slots
        UIManager.Instance.UpdateInventorySlots(slots);
    }

    public void RemoveItem(ItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {  
            if(slots[i].item == item)
            {
                slots[i].quantity --;
                
                if(slots[i].quantity == 0)
                {
                    if(UIManager.Instance.IsEquipped(i))
                    {
                        UnEquip(i);
                    }

                    slots[i].item = null;
                    ClearSelectedItemWindow();
                }

                // Update UI Slots
                UIManager.Instance.UpdateInventorySlots(slots);
                return;
            }
        }
    }

    public bool HasItems(ItemData item, int quantity)
    {
        if(slots == null) 
            return false;

        int amount = 0;

        for (int i = 0; i < slots.Length; i++)
        {  
            if(slots[i].item == item)
                amount += slots[i].quantity;

            if(amount >= quantity)
                return true;
        }

        return false;
    }
}

public class ItemSlot
{
    public ItemData item;
    public int quantity;
}
