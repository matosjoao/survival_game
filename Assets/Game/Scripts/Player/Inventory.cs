using UnityEngine;

[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(EquipManager))]
[RequireComponent(typeof(PlayerNeeds))]
public class Inventory : Singleton<Inventory>
{
    [Header("Properties")]
    [SerializeField] private Transform dropPosition;
    [SerializeField] private int inventorySize;
    [SerializeField] private int quickAccessBarSize;
    
    [Header("Components")]
    private InputReader inputReader;
    private EquipManager equipManager;
    private PlayerNeeds playerNeeds;

    [Header("Selected Item")]
    private int selectedItemIndex;
    private int quickSlotSelectedIndex = -1;

    [Header("Items")]
    public ItemSlot[] slots;
    public QuickItemSlot[] quickSlots;

    private void Awake() 
    {
        // Get componentes
        inputReader = GetComponent<InputReader>();
        equipManager = GetComponent<EquipManager>();
        playerNeeds = GetComponent<PlayerNeeds>();
    }

    private void OnEnable() 
    {
        // Subscribe to events
        inputReader.InventoryEvent += OnInventory;
        inputReader.QuickSlotClick += OnQuickSlotClick;
        inputReader.MouseClickEvent += OnConsume;
    }

    private void OnDisable() 
    {
        // Unsubscribe to events
        inputReader.InventoryEvent -= OnInventory;
        inputReader.QuickSlotClick -= OnQuickSlotClick;
        inputReader.MouseClickEvent -= OnConsume;
    }

    private void Start() 
    {
        // Close inventory window
        UIManager.Instance.ToggleInventoryWindow();
        
        // Initialize slots
        slots = new ItemSlot[inventorySize];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemSlot();
        }

        // Initialize quick slots
        quickSlots = new QuickItemSlot[quickAccessBarSize];
        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i] = new QuickItemSlot();
        }

        // Initialize UI
        UIManager.Instance.InitializeUISlots(inventorySize);
        UIManager.Instance.InitializeUIQuickSlots(quickAccessBarSize);

        // Reset selection data
        ClearSelectedItem();
    }

    private void OnDestroy() 
    {
        // TODO:: Unsubscribe to uislots events in UIMANAGER
    }

    private void OnInventory()
    {
        // Called onClick Tab to inventory
        if(UIManager.Instance.IsInventoryOpen())
        {
            UIManager.Instance.ToggleInventoryWindow();
            UIManager.Instance.ToggleCursor(false);
        }
        else
        {
            UIManager.Instance.ToggleCursor(true);
            UIManager.Instance.ToggleInventoryWindow(true);
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

                // Get item inventory position
                int slotPos = GetItemStackPosition(item);
                // Item in quick slot?
                int quickSlotItemPos = IsItemInQuickSlots(slotPos);
                if(quickSlotItemPos != -1)
                {
                    quickSlots[quickSlotItemPos].quantity++;
                    
                    // Update Quick UI Slots
                    UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
                }

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

    public void SwapItems(int toSwapIndex, int swapIndex)
    {
        // Swap items in inventory

        // Is index valid
        if(slots[toSwapIndex] == null || slots[swapIndex] == null)
            return;

        // Is swap item in quick slot ?
        int quickSlotSwapItemPos = IsItemInQuickSlots(swapIndex);
        if(quickSlotSwapItemPos != -1)
        {
            // To swap slot has item
            if(slots[toSwapIndex].item != null)
            {
                // Is to swap item in quick slot ?
                int quickSlotToSwapItemPos = IsItemInQuickSlots(toSwapIndex);
                if(quickSlotToSwapItemPos != -1)
                {
                    quickSlots[quickSlotToSwapItemPos].itemSlotPosition  = swapIndex;
                }
            }

            quickSlots[quickSlotSwapItemPos].itemSlotPosition  = toSwapIndex;
        }
        else
        {
            // To swap slot has item
            if(slots[toSwapIndex].item != null)
            {
                // Is to swap item in quick slot ?
                int quickSlotToSwapItemPos = IsItemInQuickSlots(toSwapIndex);
                if(quickSlotToSwapItemPos != -1)
                {
                    quickSlots[quickSlotToSwapItemPos].itemSlotPosition  = swapIndex;
                }
            }
        }

        // TODO:: Check if is the same item type and join

        // Swap slots
        ItemSlot item1 = slots[swapIndex];
        slots[swapIndex] = slots[toSwapIndex];
        slots[toSwapIndex] = item1;

        // Update inventory ui slots
        UIManager.Instance.UpdateInventorySlots(slots);
    }

    private void ThrowItem(ItemData item)
    {
        Instantiate(item.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360.0f));
    }

    public void OnDropButton()
    {
        // ThrowItem(selectedItem.item);
        // RemoveSelectedItem();
    }

    #region Consumables
    private void OnConsume()
    {   
        if(selectedItemIndex == -1)
            return;
        
        // Has selected item ? 
        ItemSlot itemSlot = slots[selectedItemIndex];

        if(itemSlot.item == null || !UIManager.Instance.CanLook)
            return;

        // Is consumable type
        if(itemSlot.item.type == ItemType.Consumable)
        {
            for (int i = 0; i < itemSlot.item.consumables.Length; i++)
            {
                switch (itemSlot.item.consumables[i].type)
                {
                    case ConsumableType.Health: 
                        playerNeeds.Heal(itemSlot.item.consumables[i].value);
                        break;
                    
                    case ConsumableType.Hunger: 
                        playerNeeds.Eat(itemSlot.item.consumables[i].value);
                        break;
                    
                    case ConsumableType.Thirst: 
                        playerNeeds.Drink(itemSlot.item.consumables[i].value);
                        break;

                    default:
                        return;
                }
            }

            ReduceConsumableQuantity();
        }
    }

    private void ReduceConsumableQuantity()
    {
        // Reduce in quick slot
        quickSlots[quickSlotSelectedIndex].quantity--;

        // Reduce in slot
        slots[selectedItemIndex].quantity--;

        
        // If quantity is 0 remove item
        if(slots[selectedItemIndex].quantity == 0)
        {   
            int quickSlotPos = quickSlotSelectedIndex;

            // Clear in slots
            slots[selectedItemIndex].item = null;
            slots[selectedItemIndex].quantity = 0;

            // Clear selected item
            ClearSelectedItem();

            // If is in quick slot
            quickSlots[quickSlotPos].item = null;
            quickSlots[quickSlotPos].quantity = 0;
            quickSlots[quickSlotPos].itemSlotPosition = 0;
        }

        // Update UI Slots
        UIManager.Instance.UpdateInventorySlots(slots);

        // Update Quick Slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
    }
    #endregion

    #region Equipables

    private void Equip()
    {
        equipManager.EquipNewItem(slots[selectedItemIndex].item);
    }

    private void UnEquip()
    {
        equipManager.UnEquip();
    }

    #endregion

    #region Inventory helpers functions
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

    private int GetItemStackPosition(ItemData item)
    {
        // Search for a slot of the same type
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item == item && slots[i].quantity < item.maxStackAmount)
            {
                return i;
            }
        }

        return -1;
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

    public void RemoveResourcesCosts(ItemData item, int quantity)
    {
        for (int i = 0; i < slots.Length; i++)
        {  
            if(slots[i].item == item)
            { 
                // Reduce quantity
                slots[i].quantity -= quantity;

                // Reduce quantity in quick slots if need it
                int quickSlotItemPos = IsItemInQuickSlots(i);
                if(quickSlotItemPos != -1)
                {
                    quickSlots[quickSlotItemPos].quantity -= quantity;
                }
                
                if(slots[i].quantity == 0)
                {
                    // If is selected in quick slots
                    if(quickSlotSelectedIndex != -1 && quickSlots[quickSlotSelectedIndex].itemSlotPosition == i)
                    {
                        ClearSelectedItem();
                    }
                    
                    // If is in quick slot
                    if(quickSlotItemPos != -1)
                    {
                        quickSlots[quickSlotItemPos].item = null;
                        quickSlots[quickSlotItemPos].quantity = 0;
                        quickSlots[quickSlotItemPos].itemSlotPosition = 0;
                    }

                    // Remove from slots
                    slots[i].item = null;
                }


                // Update UI Slots
                UIManager.Instance.UpdateInventorySlots(slots);

                // Update UI Slots
                UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
                return;
            }
        }
    }
    #endregion

    #region Quick Slots
    public void AddToQuickSlot(int targetQuickSlotIndex, int inventoryItemIndex)
    {
        // Is index valid
        if(quickSlots[targetQuickSlotIndex] == null || slots[inventoryItemIndex] == null)
            return;
        
        // If is resource don't allow drop
        if(slots[inventoryItemIndex].item.type == ItemType.Resource)
            return;

        // Is Item already in quick slots
        int itemQuickSlotPos = IsItemInQuickSlots(inventoryItemIndex);

        // Dragged to the same position
        if(itemQuickSlotPos == targetQuickSlotIndex)
            return;

        // If target quick slot position equals selected position
        if(quickSlotSelectedIndex == targetQuickSlotIndex)
        {
            // If item in target slot is equipable
            SelectItem(inventoryItemIndex, targetQuickSlotIndex);

            
            // New item is equipable
            if(slots[inventoryItemIndex].item.type == ItemType.Equipable)
                Equip();
        }

        // If item already in quick slot and is old position is equal to selected position
        if(itemQuickSlotPos != -1 && quickSlotSelectedIndex == itemQuickSlotPos)
        {
            // Has item on target quick slot
            if(quickSlots[targetQuickSlotIndex].item != null)
            {
                // If item in quick slot is selected
                SelectItem(quickSlots[targetQuickSlotIndex].itemSlotPosition, itemQuickSlotPos);

                // New item is equipable
                if(quickSlots[targetQuickSlotIndex].item.type == ItemType.Equipable)
                    Equip();
            }
            else
            {
                ClearSelectedItem();
            }
            
        }

        // If item dragged already in quick slots
        if(itemQuickSlotPos != -1)
        {
            // Is in quick slot already
            // Assign to new slot
            QuickItemSlot item1 = quickSlots[targetQuickSlotIndex];
            quickSlots[targetQuickSlotIndex] = quickSlots[itemQuickSlotPos];
            quickSlots[itemQuickSlotPos] = item1;
        }
        else
        {
            // Is not in quick slot
            // Assign to slot
            quickSlots[targetQuickSlotIndex].item = slots[inventoryItemIndex].item;
            quickSlots[targetQuickSlotIndex].quantity = slots[inventoryItemIndex].quantity;
            quickSlots[targetQuickSlotIndex].itemSlotPosition = inventoryItemIndex;
        }
        
        // Update Quick UI Slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
    }

    private void OnQuickSlotClick(int pos)
    {
        // Has item in slot? Or clicked the same position?
        if(quickSlots[pos] == null || quickSlots[pos].item == null || pos == quickSlotSelectedIndex)
        {
            ClearSelectedItem();
            return;
        }

        // Save item
        QuickItemSlot itemSlot = quickSlots[pos];

        // Set selected item
        SelectItem(itemSlot.itemSlotPosition, pos);

        switch (itemSlot.item.type)
        {
            case ItemType.Equipable: 
                Equip();
                break;
            
            case ItemType.Building: 
                // Start Building Preview
                break;
            
            case ItemType.Consumable: 
                break;

            default:
                return;
        }
        
        // Update UI Quick Slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
    }

    private void SelectItem(int inventoryIndex, int quickSlotIndex)
    {
        if(quickSlotSelectedIndex != -1)
        {
            // If Old selected is equipable
            if(quickSlots[quickSlotSelectedIndex].item.type == ItemType.Equipable)
            {
                // UnEquip
                UnEquip();
            }

            // UnSelect Old Quick Slots UI's
            UIManager.Instance.SetQuickSlotSelected(quickSlotSelectedIndex, false);
        }

        // Item in inventory
        selectedItemIndex = inventoryIndex;

        // Item in Quick Slot
        quickSlotSelectedIndex = quickSlotIndex;

        // Update Quick Slots UI's
        UIManager.Instance.SetQuickSlotSelected(quickSlotIndex, true);
    }

    private void ClearSelectedItem()
    {
        if(quickSlotSelectedIndex != -1)
        {
            // If Old selected is equipable
            if(quickSlots[quickSlotSelectedIndex].item.type == ItemType.Equipable)
            {
                // UnEquip
                UnEquip();
            }

            // UnSelect Old Quick Slots UI's
            UIManager.Instance.SetQuickSlotSelected(quickSlotSelectedIndex, false);

            // Update UI Quick Slots
            UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
        }

        // Item in inventory
        selectedItemIndex = -1;

        // Item in Quick Slot
        quickSlotSelectedIndex = -1;
    }

    private int IsItemInQuickSlots(int position)
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if(quickSlots[i].item != null && quickSlots[i].itemSlotPosition == position)
            {
                return i;
            }
        }

        return -1;
    }
    #endregion
}

[System.Serializable]
public class ItemSlot
{
    public ItemData item;
    public int quantity;
}

[System.Serializable]
public class QuickItemSlot
{
    public ItemData item;
    public int itemSlotPosition;
    public int quantity;
}
