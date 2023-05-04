using System;
using UnityEngine;

[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(EquipManager))]
[RequireComponent(typeof(PlayerNeeds))]
[RequireComponent(typeof(PlayerBuild))]
[RequireComponent(typeof(PlayerController))]
public class Inventory : Singleton<Inventory>
{
    [Header("Properties")]
    [SerializeField] private Transform dropPosition;
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private int inventorySize;
    [SerializeField] private int quickAccessBarSize;

    [Header("Components")]
    private InputReader inputReader;
    private EquipManager equipManager;
    private PlayerNeeds playerNeeds;
    private PlayerBuild playerBuild;
    private PlayerController playerController;

    [Header("Selected Item")]
    private int selectedItemIndex;
    private int quickSlotSelectedIndex = -1;

    [Header("Items")]
    private ItemSlot[] slots;
    private QuickItemSlot[] quickSlots;

    [Header("Current Interactables")]
    public DroppedBag curDroppedBag; // TODO:: Change to private and ad a function that changes this value
    private BaseInventory curInteractable;

    private void Awake() 
    {
        // Get componentes
        inputReader = GetComponent<InputReader>();
        equipManager = GetComponent<EquipManager>();
        playerNeeds = GetComponent<PlayerNeeds>();
        playerBuild = GetComponent<PlayerBuild>();
        playerController = GetComponent<PlayerController>();

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
        // UIManager.Instance.InitializeUISlots(inventorySize);
        // UIManager.Instance.InitializeUIQuickSlots(quickAccessBarSize);
    }

    private void OnEnable() 
    {
        // Subscribe to events
        inputReader.InventoryEvent += OnInventory;
        inputReader.QuickSlotClick += OnQuickSlotClick;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].OnItemDrop += DropItem;
            slots[i].OnItemAdd += AddItemFromStorage;
            slots[i].OnItemSwap += SwapItems;
        }  

        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].OnItemAddToQuickSlot += HandleAddToQuickSlot;
        }
    }

    private void OnDisable() 
    {
        // Unsubscribe to events
        inputReader.InventoryEvent -= OnInventory;
        inputReader.QuickSlotClick -= OnQuickSlotClick;

        // AddItemFromStorageToQuickSlot
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].OnItemDrop -= DropItem;
            slots[i].OnItemAdd -= AddItemFromStorage;
            slots[i].OnItemSwap -= SwapItems;
        }

        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].OnItemAddToQuickSlot -= HandleAddToQuickSlot;
        }
    }

    private void Start() 
    {
        // Close inventory window
        UIManager.Instance.ToggleInventoryWindow();
        
        // Update quick slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);

        // Reset selection data
        ClearSelectedItem();
    }

    #region  Inventory
    private void OnInventory()
    {
        // Called onClick Tab to inventory
        if(UIManager.Instance.IsInventoryOpen())
        {
            // Close inventory
            UIManager.Instance.ToggleInventoryWindow();

            // Toggle cursor to false
            playerController.ToggleCursor(false);

            // Toggle IsInteraction to false
            playerController.ToggleInteract(false);

            // Toggle Storage UI
            StorageUI.Instance.Toogle();
        }
        else
        {
            // Open inventory
            UIManager.Instance.ToggleInventoryWindow(true, true);

            // Toggle cursor to true
            playerController.ToggleCursor(true);

            // Toggle IsInteraction to true
            playerController.ToggleInteract(true);
        }
    }

    public void SetCurrentInteractable(GameObject interactable)
    {
        if(interactable == null)
            return;

        if(interactable.TryGetComponent<BaseInventory>(out BaseInventory baseInventory))
        {
            curInteractable = baseInventory;
        }
    }

    public bool AddItem(ItemData item)
    {
        // Add item to inventory

        // Can stack
        if(item.canStack)
        {
            // Get item inventory position
            int slotToStackToPos = GetItemStackPosition(item);
            if(slotToStackToPos != -1)
            {
                slots[slotToStackToPos].AddQuantity(1);
                
                // Item in quick slot?
                int quickSlotItemPos = IsItemInQuickSlots(slotToStackToPos);
                if(quickSlotItemPos != -1)
                {
                    quickSlots[quickSlotItemPos].AddQuantity(1);

                    // Update Quick UI Slots
                    UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
                }

                // Update UI Slots
                UIManager.Instance.UpdateInventorySlots(slots);
                return true;
            }
        }

        // Has empty slot
        int emptySlotPos = GetEmptySlotPosition();
        if(emptySlotPos != -1)
        {
            slots[emptySlotPos].UpdateSlot(item, 1);

            // Update UI Slots
            UIManager.Instance.UpdateInventorySlots(slots);
            return true;
        }

        return false;
    }

    public void SwapItems(Guid id, int swapIndex)
    {
        // Get inventory slot index
        int toSwapIndex = GetItemPositionById(id);

        // Is index valid
        if(toSwapIndex == -1 || slots[toSwapIndex] == null || slots[swapIndex] == null)
            return;

        // Is the same type and can stack?
        if(slots[toSwapIndex].Item == slots[swapIndex].Item && slots[swapIndex].Item.canStack && slots[toSwapIndex].RoomLeftInStack(slots[swapIndex].Quantity, out int roomLeftQuantity))
        {
            // Calculate quantity to add
            int quantityToAdd = Mathf.Min(roomLeftQuantity, slots[swapIndex].Quantity);

            // Exchange quantities
            slots[toSwapIndex].AddQuantity(quantityToAdd);
            slots[swapIndex].RemoveQuantity(quantityToAdd);

            // Quick Slot To Swap item
            int quickSlotToSwapItemPos = IsItemInQuickSlots(toSwapIndex);
            if(quickSlotToSwapItemPos != -1)
            {
                quickSlots[quickSlotToSwapItemPos].AddQuantity(quantityToAdd);
            }

            // Quick Slot Swap item
            int quickSlotSwapItemPos = IsItemInQuickSlots(swapIndex);
            if(quickSlotSwapItemPos != -1)
            {
                quickSlots[quickSlotSwapItemPos].RemoveQuantity(quantityToAdd);
            }

            // Has quantity in slot?
            if(slots[swapIndex].Quantity == 0)
            {
                // If is in quick slot
                if(quickSlotSwapItemPos != -1)
                {
                    // Is the selected item
                    if(swapIndex == selectedItemIndex)
                    {
                        ClearSelectedItem();
                    }

                    quickSlots[quickSlotSwapItemPos].ClearSlot();
                }

                // Clear Slot
                slots[swapIndex].ClearSlot();
            }
        }
        else
        {
            // Is swap item in quick slot ?
            int quickSlotSwapItemPos = IsItemInQuickSlots(swapIndex);

            if(quickSlotSwapItemPos != -1)
            {
                // To swap slot has item
                if(slots[toSwapIndex].Item != null)
                {
                    // Is to swap item in quick slot ?
                    int quickSlotToSwapItemPos = IsItemInQuickSlots(toSwapIndex);
                    if(quickSlotToSwapItemPos != -1)
                    {
                        quickSlots[quickSlotToSwapItemPos].SetItemSlotPosition(swapIndex);
                    }
                }

                quickSlots[quickSlotSwapItemPos].SetItemSlotPosition(toSwapIndex);
            }
            else
            {
                // To swap slot has item
                if(slots[toSwapIndex].Item != null)
                {
                    // Is to swap item in quick slot ?
                    int quickSlotToSwapItemPos = IsItemInQuickSlots(toSwapIndex);
                    if(quickSlotToSwapItemPos != -1)
                    {
                        quickSlots[quickSlotToSwapItemPos].SetItemSlotPosition(swapIndex);
                    }
                }
            }

            // Change selectedItemIndex
            if(selectedItemIndex == swapIndex)
            {
                selectedItemIndex = toSwapIndex;
            }
            else if(selectedItemIndex == toSwapIndex)
            {
                selectedItemIndex = swapIndex;
            }

            // Swap slots
            ItemSlot item1 = slots[swapIndex];
            slots[swapIndex] = slots[toSwapIndex];
            slots[toSwapIndex] = item1;
        }

        // Update inventory ui slots
        UIManager.Instance.UpdateInventorySlots(slots);

        // Update Quick Slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
    }
    
    public void DropItem(Guid id)
    {
        // Drop item from inventory

        // Get inventory slot index
        int dropIndex = GetItemPositionById(id);

        // Is index valid
        if(dropIndex == -1 || slots[dropIndex] == null || slots[dropIndex].Item == null)
            return;
        
        // Drop a bag
        if(curDroppedBag == null)
        {
            // New Bag
            // TODO:: Improve
            // Get Dropped bag from pool
            // listen to an event when the dropped bag is realease or empty and set Inventory droppedbag to null
            curDroppedBag = Instantiate(dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * UnityEngine.Random.value * 360.0f)).GetComponent<DroppedBag>();
        }
        
        // Add Drop item
        if(curDroppedBag.AddDroppedItem(slots[dropIndex].Item, slots[dropIndex].Quantity))
        {
            RemoveItem(dropIndex);
        }
    }

    public void RemoveItem(int index)
    {
        // Is index valid
        if(slots[index] == null || slots[index].Item == null)
            return;
        
        // Clear in slots
        slots[index].ClearSlot();

        // If is in quick slot
        int quickSlotItemPos = IsItemInQuickSlots(index);
        if(quickSlotItemPos != -1)
        {
            // Is the selected item
            if(index == selectedItemIndex)
            {
                ClearSelectedItem();
            }

            quickSlots[quickSlotItemPos].ClearSlot();
        }

        // Update UI Slots
        UIManager.Instance.UpdateInventorySlots(slots);

        // Update Quick Slots
        if(quickSlotItemPos != -1)
        {
            UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
        }
    }

    public void UpdateSlotQuantity(int index, int amount)
    {
        // Is index valid
        if(slots[index] == null || slots[index].Item == null)
            return;

        // Remove quantity from slots
        slots[index].RemoveQuantity(amount);

        // If is in quick slot
        int quickSlotItemPos = IsItemInQuickSlots(index);
        if(quickSlotItemPos != -1)
        {
            quickSlots[quickSlotItemPos].RemoveQuantity(amount);
        }

        // Has quantity in slot?
        if(slots[index].Quantity == 0)
        {
            // If is in quick slot
            if(quickSlotItemPos != -1)
            {
                // Is the selected item
                if(index == selectedItemIndex)
                {
                    ClearSelectedItem();
                }

                quickSlots[quickSlotItemPos].ClearSlot();
            }

            // Clear Slot
            slots[index].ClearSlot();
        }

        // Update UI Slots
        UIManager.Instance.UpdateInventorySlots(slots);

        // Update Quick Slots
        if(quickSlotItemPos != -1)
        {
            UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
        }
    }

    public void UpdateSlot(ItemSlot itemSlot, int index)
    {
        // Is index valid
        if(slots[index] == null)
            return;

        // Update slot
        slots[index].UpdateSlot(itemSlot.Item, itemSlot.Quantity);

        // If is in quick slot
        int quickSlotItemPos = IsItemInQuickSlots(index);
        if(quickSlotItemPos != -1)
        {
            // New item can be in quick slots
            if(itemSlot.Item.type == ItemType.Resource)
            {
                // Is the selected item
                if(index == selectedItemIndex)
                {
                    ClearSelectedItem();
                }

                quickSlots[quickSlotItemPos].ClearSlot();
            }
            else
            {
                // Is the selected item
                if(index == selectedItemIndex)
                {
                    SelectItem(index, quickSlotItemPos);

                    // New item is equipable
                    if(itemSlot.Item.type == ItemType.Equipable)
                        Equip();

                    // New item is building
                    if(itemSlot.Item.type == ItemType.Building)
                        StartBuilding();
                }

                // Update quick slot
                quickSlots[quickSlotItemPos].UpdateSlot(itemSlot.Item, itemSlot.Quantity);
            }
        }

        // Update UI Slots
        UIManager.Instance.UpdateInventorySlots(slots);

        // Update Quick Slots
        if(quickSlotItemPos != -1)
        {
            UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
        }
    }

    public void AddItemFromStorage(Guid id, int storageIndex)
    {
        // Add item in inventory from storage

        // Is interacting with something
        if(curInteractable == null)
            return;

        // Get inventory slot index
        int inventoryIndex = GetItemPositionById(id);

        // Get item data
        ItemSlot storageSlot = curInteractable.GetItemInSlot(storageIndex);
        if(storageSlot == null || storageSlot.Item == null)
            return;

        // Is index valid
        if(inventoryIndex == -1 || slots[inventoryIndex] == null)
            return;

        // Has item?
        if(slots[inventoryIndex].Item == null)
        {
            // Don't have item

            // Update Slot
            slots[inventoryIndex].UpdateSlot(storageSlot.Item, storageSlot.Quantity);

            // Remove from storage
            curInteractable.RemoveItem(storageIndex);

            // Update quick ui slots
            UIManager.Instance.UpdateInventorySlots(slots); 
        }
        else
        {
            // Are the same type and can stack and quantity in slot is less then max stack amount
            if(slots[inventoryIndex].Item == storageSlot.Item && storageSlot.Item.canStack && slots[inventoryIndex].RoomLeftInStack(storageSlot.Quantity, out int roomLeftQuantity ))
            {
                // Calculate quantity to add
                int quantityToAdd = Mathf.Min(roomLeftQuantity, storageSlot.Quantity);

                // Exchange quantities
                slots[inventoryIndex].AddQuantity(quantityToAdd);

                // If is in quick slot
                int quickSlotItemPos = IsItemInQuickSlots(inventoryIndex);
                if(quickSlotItemPos != -1)
                {
                    quickSlots[quickSlotItemPos].AddQuantity(quantityToAdd);
                }

                // Update quantity in inventory
                curInteractable.UpdateSlotQuantity(storageIndex, quantityToAdd);

                // Update ui slots
                UIManager.Instance.UpdateInventorySlots(slots); 

                if(quickSlotItemPos != -1)
                {
                    // Update ui quick slots
                    UIManager.Instance.UpdateInventoryQuickSlots(quickSlots); 
                }
            }
            else
            {
                // Swap items
                ItemSlot item1 = new ItemSlot(slots[inventoryIndex].Item, slots[inventoryIndex].Quantity);

                // Assign item to inventory slot
                UpdateSlot(storageSlot, inventoryIndex);

                // Assign item to storage slot
                curInteractable.UpdateSlot(item1, storageIndex);
            }
        }
    }
    #endregion

    #region Building
    private void UnEquipBuild()
    {
        playerBuild.StopBuilding();
    }

    private void StartBuilding()
    {
        playerBuild.StartBuilding(slots[selectedItemIndex].Item);
    }
    #endregion

    #region Equipables

    private void Equip()
    {
        equipManager.EquipNewItem(slots[selectedItemIndex].Item);
    }

    private void UnEquip()
    {
        equipManager.UnEquip();
    }

    #endregion

    #region Inventory helpers functions
    private int GetItemStackPosition(ItemData item)
    {
        // Search for a slot of the same type
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].Item == item && slots[i].Quantity < item.maxStackAmount)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetItemPositionById(Guid id)
    {
        // Search for a slot of the same ID
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetEmptySlotPosition()
    {
        // Search for a empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].Item == null)
            {
                return i;
            }
        }

        return -1;
    }

    public ItemSlot GetItemInSlot(int index)
    {
        return slots[index];
    }

    public ItemSlot GetSelectedItemSlot()
    {
        if(selectedItemIndex == -1)
            return null;
            
        return slots[selectedItemIndex];
    }

    public bool HasItems(ItemData item, int quantity)
    {
        if(slots == null) 
            return false;

        int amount = 0;

        for (int i = 0; i < slots.Length; i++)
        {  
            if(slots[i].Item == item)
                amount += slots[i].Quantity;

            if(amount >= quantity)
                return true;
        }

        return false;
    }

    public void RemoveResourcesCosts(ItemData item, int quantity)
    {
        int quantityToRemove = quantity;

        for (int i = 0; i < slots.Length; i++)
        {  
            if(slots[i].Item == item)
            { 
                // Calculate quantity to remove
                int amount = quantityToRemove;
                // Check if the quantity that we have in the slot is less then the quantity to remove
                if(slots[i].Quantity < quantityToRemove)
                {
                    amount = slots[i].Quantity;
                    quantityToRemove -= amount;
                }

                // Reduce quantity
                slots[i].RemoveQuantity(amount);

                // Reduce quantity in quick slots if need it
                int quickSlotItemPos = IsItemInQuickSlots(i);
                if(quickSlotItemPos != -1)
                {
                    quickSlots[quickSlotItemPos].RemoveQuantity(quantity);
                }
                
                if(slots[i].Quantity == 0)
                {
                    // If is selected in quick slots
                    if(quickSlotSelectedIndex != -1 && quickSlots[quickSlotSelectedIndex].ItemSlotPosition == i)
                    {
                        ClearSelectedItem();
                    }
                    
                    // If is in quick slot
                    if(quickSlotItemPos != -1)
                    {
                        quickSlots[quickSlotItemPos].ClearSlot();
                    }

                    // Remove from slots
                    slots[i].ClearSlot();
                }


                // Update UI Slots
                UIManager.Instance.UpdateInventorySlots(slots);

                // Update UI Slots
                UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
                return;
            }
        }
    }

    public bool OnActionReduceSelectedItemQuantity()
    {
        // Reduce in quick slot
        quickSlots[quickSlotSelectedIndex].RemoveQuantity(1);

        // Reduce in slot
        slots[selectedItemIndex].RemoveQuantity(1);

        // If quantity is 0 remove item
        if(slots[selectedItemIndex].Quantity == 0)
        {   
            int quickSlotPos = quickSlotSelectedIndex;

            // Clear in slots
            slots[selectedItemIndex].ClearSlot();

            // Clear selected item
            ClearSelectedItem();

            // If is in quick slot
            quickSlots[quickSlotPos].ClearSlot();

            return true;
        }

        // Update UI Slots
        UIManager.Instance.UpdateInventorySlots(slots);

        // Update Quick Slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);

        return slots[selectedItemIndex].Quantity == 0;
    } 

    #endregion

    #region Quick Slots
    private void HandleAddToQuickSlot(Guid id, int dPos, bool isInv)
    {
        if(isInv)
        {
            AddToQuickSlot(id, dPos);
        }
        else
        {
            AddItemFromStorageToQuickSlot(id, dPos);
        }
    }

    public void AddToQuickSlot(Guid id, int inventoryItemIndex) 
    {
        // Get inventory slot index
        int targetQuickSlotIndex = GetQuickSlotItemPositionById(id);

        // Is index valid
        if(targetQuickSlotIndex == -1 || quickSlots[targetQuickSlotIndex] == null || slots[inventoryItemIndex] == null)
            return;
        
        // If is resource don't allow drop
        if(slots[inventoryItemIndex].Item.type == ItemType.Resource)
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
            if(slots[inventoryItemIndex].Item.type == ItemType.Equipable)
                Equip();

            // TODO:: We could have Build()... 
        }

        // If item already in quick slot and is old position is equal to selected position
        if(itemQuickSlotPos != -1 && quickSlotSelectedIndex == itemQuickSlotPos)
        {
            // Has item on target quick slot
            if(quickSlots[targetQuickSlotIndex].Item != null)
            {
                // If item in quick slot is selected
                SelectItem(quickSlots[targetQuickSlotIndex].ItemSlotPosition, itemQuickSlotPos);

                // New item is equipable
                if(quickSlots[targetQuickSlotIndex].Item.type == ItemType.Equipable)
                    Equip();

                // TODO:: We could have Build()... 
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
            quickSlots[targetQuickSlotIndex].UpdateQuickSlot(slots[inventoryItemIndex].Item, slots[inventoryItemIndex].Quantity, inventoryItemIndex);
        }
        
        // Update Quick UI Slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
    }

    public void AddItemFromStorageToQuickSlot(Guid id, int storageIndex)
    {
        // Add item in inventory from storage

        // Is interacting with something
        if(curInteractable == null)
            return;

        // Get inventory slot index
        int targetQuickSlotIndex = GetQuickSlotItemPositionById(id);

        // Is index valid
        if(targetQuickSlotIndex == -1 || quickSlots[targetQuickSlotIndex] == null)
            return;

        // Get item data
        ItemSlot storageSlot = curInteractable.GetItemInSlot(storageIndex);
        if(storageSlot == null || storageSlot.Item == null)
            return;

        // If is resource don't allow drop
        if(storageSlot.Item.type == ItemType.Resource)
            return;

        // Has item?
        if(quickSlots[targetQuickSlotIndex].Item == null)
        {
            // Don't have item

            // Has empty slot
            int emptySlotPos = GetEmptySlotPosition();
            if(emptySlotPos == -1)
                return;

            // Add item to slots
            slots[emptySlotPos].UpdateSlot(storageSlot.Item, storageSlot.Quantity);

            // Update Quick Slot
            quickSlots[targetQuickSlotIndex].UpdateQuickSlot(storageSlot.Item, storageSlot.Quantity, emptySlotPos);

            // Remove from storage
            curInteractable.RemoveItem(storageIndex);
        }
        else
        {
            // Are the same type and can stack and quantity in slot is less then max stack amount
            if(quickSlots[targetQuickSlotIndex].Item == storageSlot.Item && storageSlot.Item.canStack && quickSlots[targetQuickSlotIndex].RoomLeftInStack(storageSlot.Quantity, out int roomLeftQuantity ))
            {
                // Calculate quantity to add
                int quantityToAdd = Mathf.Min(roomLeftQuantity, storageSlot.Quantity);

                // Exchange quantities
                int slotPos = quickSlots[targetQuickSlotIndex].ItemSlotPosition;
                if(slots[slotPos] == null)
                    return;

                quickSlots[targetQuickSlotIndex].AddQuantity(quantityToAdd);
                slots[slotPos].AddQuantity(quantityToAdd);

                // Update quantity in inventory
                curInteractable.UpdateSlotQuantity(storageIndex, quantityToAdd);
            }
            else
            {
                // Has empty slot
                int emptySlotPos = GetEmptySlotPosition();
                if(emptySlotPos == -1)
                    return;

                // Add item to slots
                slots[emptySlotPos].UpdateSlot(storageSlot.Item, storageSlot.Quantity);

                // If target quick slot position equals selected position
                if(quickSlotSelectedIndex == targetQuickSlotIndex)
                {
                    // If item in target slot is equipable
                    SelectItem(emptySlotPos, targetQuickSlotIndex);

                    // New item is equipable
                    if(slots[emptySlotPos].Item.type == ItemType.Equipable)
                        Equip();

                    // TODO:: We could have Build()... 
                }
                
                // Update quick slot
                quickSlots[targetQuickSlotIndex].UpdateQuickSlot(storageSlot.Item, storageSlot.Quantity, emptySlotPos);

                // Remove from storage
                curInteractable.RemoveItem(storageIndex);
            }
        }

        // Update ui slots
        UIManager.Instance.UpdateInventorySlots(slots); 

        // Update quick ui slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
    } 

    private void OnQuickSlotClick(int pos)
    {
        // Has item in slot? Or clicked the same position?
        if(quickSlots[pos] == null || quickSlots[pos].Item == null || pos == quickSlotSelectedIndex)
        {
            ClearSelectedItem();
            return;
        }

        // Save item
        QuickItemSlot itemSlot = quickSlots[pos];

        // Set selected item
        SelectItem(itemSlot.ItemSlotPosition, pos);

        switch (itemSlot.Item.type)
        {
            case ItemType.Equipable: 
                // Equip item
                Equip();
                break;
            
            case ItemType.Building: 
                // Start Building Preview
                StartBuilding();
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
            if(quickSlots[quickSlotSelectedIndex].Item.type == ItemType.Equipable)
            {
                // UnEquip
                UnEquip();
            }

            // If Old selected is building
            if(quickSlots[quickSlotSelectedIndex].Item.type == ItemType.Building)
            {
                // UnBuild
                UnEquipBuild();
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
            if(quickSlots[quickSlotSelectedIndex].Item.type == ItemType.Equipable)
            {
                // UnEquip
                UnEquip();
            }

            // If Old selected is building
            if(quickSlots[quickSlotSelectedIndex].Item.type == ItemType.Building)
            {
                // UnBuild
                UnEquipBuild();
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
            if(quickSlots[i].Item != null && quickSlots[i].ItemSlotPosition == position)
            {
                return i;
            }
        }

        return -1;
    } 

    private int GetQuickSlotItemPositionById(Guid id)
    {
        // Search for a slot of the same ID
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if(quickSlots[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }
    #endregion
}