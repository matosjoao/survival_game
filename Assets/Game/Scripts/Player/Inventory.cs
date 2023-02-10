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
    public ItemSlot[] slots;
    public QuickItemSlot[] quickSlots;

    [Header("Dropped Bag")]
    private DroppedBag droppedBag;

    private void Awake() 
    {
        // Get componentes
        inputReader = GetComponent<InputReader>();
        equipManager = GetComponent<EquipManager>();
        playerNeeds = GetComponent<PlayerNeeds>();
        playerBuild = GetComponent<PlayerBuild>();
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable() 
    {
        // Subscribe to events
        inputReader.InventoryEvent += OnInventory;
        inputReader.QuickSlotClick += OnQuickSlotClick;
        inputReader.MouseClickEvent += OnMouseClickEvent;
    }

    private void OnDisable() 
    {
        // Unsubscribe to events
        inputReader.InventoryEvent -= OnInventory;
        inputReader.QuickSlotClick -= OnQuickSlotClick;
        inputReader.MouseClickEvent -= OnMouseClickEvent;
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
        UIManager.Instance.InitializeDropzone();

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

            // Toggle Bag UI
            ItemsSlotsUI.Instance.Toogle();
            // Toggle Interaction
            UIManager.Instance.ToggleInteract(true);
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
        int emptySlotPos = GetEmptySlotPosition();
        if(emptySlotPos != -1)
        {
            slots[emptySlotPos].item = item;
            slots[emptySlotPos].quantity = 1;

            // Update UI Slots
            UIManager.Instance.UpdateInventorySlots(slots);
            return;
        }

        // TODO:: Improve maybe dont get it if can't stack and no empty slot
        //ThrowItem(item);
    }

    public void SwapItems(int toSwapIndex, int swapIndex)
    {
        // Swap items in inventory

        // Is index valid
        if(slots[toSwapIndex] == null || slots[swapIndex] == null)
            return;

        // TODO:: Check if is the same item type and join

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

        // Swap slots
        ItemSlot item1 = slots[swapIndex];
        slots[swapIndex] = slots[toSwapIndex];
        slots[toSwapIndex] = item1;

        // Update inventory ui slots
        UIManager.Instance.UpdateInventorySlots(slots);
    }

    public void DropItem(int index)
    {
        // Is index valid
        if(slots[index] == null || slots[index].item == null)
            return;
        
        // Drop a bag
        if(droppedBag == null)
        {
            droppedBag = Instantiate(dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * UnityEngine.Random.value * 360.0f)).GetComponent<DroppedBag>();
        }

        if(droppedBag.CanAddItem())
        {
            // Add item to dropped bag
            droppedBag.AddItem(slots[index].item, slots[index].quantity);

            // Clear in slots
            slots[index].item = null;
            slots[index].quantity = 0;

            // If is in quick slot
            int quickSlotItemPos = IsItemInQuickSlots(index);

            // Is the selected item
            if(index == selectedItemIndex)
            {
                ClearSelectedItem();
            }
            
            if(quickSlotItemPos != -1)
            {
                quickSlots[quickSlotItemPos].item = null;
                quickSlots[quickSlotItemPos].quantity = 0;
                quickSlots[quickSlotItemPos].itemSlotPosition = 0;
            }

            // Update UI Slots
            UIManager.Instance.UpdateInventorySlots(slots);

            // Update Quick Slots
            UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
        }
    }

    public void AddItemFromBag(int toSwapIndex, ItemSlot bagSlot)
    {
        // Is index valid
        if(slots[toSwapIndex] == null)
            return;

        // Has item
        if(slots[toSwapIndex].item != null)
        {

            // Are the same type and can stack and quantity in slot is less then max stack amount
            if(slots[toSwapIndex].item == bagSlot.item && bagSlot.item.canStack && (slots[toSwapIndex].quantity < bagSlot.item.maxStackAmount))
            {
                // Quantity to add plus quantity in slot is greater then max stack amount
                if((slots[toSwapIndex].quantity + bagSlot.quantity) >  bagSlot.item.maxStackAmount)
                {
                    int qtdToAdd = bagSlot.quantity;

                    // Assign quantity to first slot
                    int qtdFirstSlot = bagSlot.item.maxStackAmount - slots[toSwapIndex].quantity;
                    slots[toSwapIndex].quantity += qtdFirstSlot;

                    // Remove quantity
                    qtdToAdd -= qtdFirstSlot;

                    // If is in quick slot
                    int quickSlotItemPos = IsItemInQuickSlots(toSwapIndex);

                    // Assign quantity to quick slot
                    if(quickSlotItemPos != -1)
                    {
                        quickSlots[quickSlotItemPos].quantity += qtdFirstSlot;
                    }


                    // Steel has quantity
                    while(qtdToAdd > 0)
                    {

                        int slotToStackToPos = GetItemStackPosition(bagSlot.item);

                        int emptySlotPos = GetEmptySlotPosition();

                        if(slotToStackToPos != -1)
                        {
                            // Assign quantity to slot
                            int qtdOtherSlot = bagSlot.item.maxStackAmount - slots[slotToStackToPos].quantity;
                            slots[slotToStackToPos].quantity += Mathf.Min(qtdToAdd, qtdOtherSlot);

                            // Item in quick slot?
                            int quickSlotItemOtherPos = IsItemInQuickSlots(slotToStackToPos);
                            if(quickSlotItemOtherPos != -1)
                            {
                                quickSlots[quickSlotItemOtherPos].quantity += Mathf.Min(qtdToAdd, qtdOtherSlot);
                            }

                            // Remove quantity
                            qtdToAdd -= qtdOtherSlot;
                        }
                        else if(emptySlotPos != -1)
                        {
                            // Has empty slot

                            int qtdOtherSlot = Mathf.Min(qtdToAdd, bagSlot.item.maxStackAmount);

                            slots[emptySlotPos].item = bagSlot.item;
                            slots[emptySlotPos].quantity = qtdOtherSlot;

                            // Remove quantity
                            qtdToAdd -= qtdOtherSlot;
                        }

                        // Can't add to inventory
                        if(slotToStackToPos == -1 && emptySlotPos == -1)
                            return;
                    }

                    // Remove item from bag
                    bagSlot.ItemSlotDraggedToInventory();

                    if(qtdToAdd > 0)
                    {
                        // Add item to dropped bag
                        droppedBag.AddItem(slots[toSwapIndex].item, qtdToAdd);
                    }
                }
                else
                {
                    // Update quantity in slot
                    slots[toSwapIndex].quantity += bagSlot.quantity;

                    // If is in quick slot
                    int quickSlotItemPos = IsItemInQuickSlots(toSwapIndex);

                    // Assign quantity to quick slot
                    if(quickSlotItemPos != -1)
                    {
                        quickSlots[quickSlotItemPos].quantity += bagSlot.quantity;
                    }

                    // Remove item from bag
                    bagSlot.ItemSlotDraggedToInventory();
                }
            }
            else
            {
                // Swap items

                ItemSlot itemToSave = bagSlot;
                
                // Remove item from bag
                bagSlot.ItemSlotDraggedToInventory();

                // Add item to dropped bag
                droppedBag.AddItem(slots[toSwapIndex].item, slots[toSwapIndex].quantity);

                // If is in quick slot
                int quickSlotItemPos = IsItemInQuickSlots(toSwapIndex);

                // Assign to inventory
                slots[toSwapIndex] = itemToSave;
            
                // Is the selected item
                if(toSwapIndex == selectedItemIndex)
                {
                    SelectItem(toSwapIndex, quickSlotItemPos);

                    // New item is equipable
                    if(itemToSave.item.type == ItemType.Equipable)
                        Equip();

                    // New item is building
                    if(itemToSave.item.type == ItemType.Building)
                        StartBuilding();
                }
                
                // Assign data to quick slot
                if(quickSlotItemPos != -1)
                {
                    quickSlots[quickSlotItemPos].item = itemToSave.item;
                    quickSlots[quickSlotItemPos].quantity = itemToSave.quantity;
                }
            }
        }
        else
        {
            // Don't have item
            slots[toSwapIndex] = bagSlot;
            bagSlot.ItemSlotDraggedToInventory();
        }

        // Update quick ui slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots); 

        // Update inventory ui slots
        UIManager.Instance.UpdateInventorySlots(slots); 
    }

    public void AddItemFromBagToQuickSlot(int targetQuickSlotIndex, ItemSlot bagSlot)
    {
        // Is index valid
        if(quickSlots[targetQuickSlotIndex] == null)
            return;

        // If is resource don't allow drop
        if(bagSlot.item.type == ItemType.Resource)
            return;

        QuickItemSlot quickSlotTarget = quickSlots[targetQuickSlotIndex];

        // Can stack
        if(bagSlot.item.canStack)
        {
            int slotToStackToPos = GetItemStackPosition(bagSlot.item);
            if(slotToStackToPos != -1)
            {
                int qtdToAdd = bagSlot.quantity;

                // Assign quantity to slot
                int qtdFirstSlot = bagSlot.item.maxStackAmount - slots[quickSlotTarget.itemSlotPosition].quantity;
                slots[quickSlotTarget.itemSlotPosition].quantity += qtdFirstSlot;

                // Assign quantity to quick slot
                quickSlots[targetQuickSlotIndex].quantity += qtdFirstSlot;

                // Remove quantity
                qtdToAdd -= qtdFirstSlot;

                // Steel has quantity
                while(qtdToAdd > 0)
                {

                    int slotToStackToOtherPos = GetItemStackPosition(bagSlot.item);

                    int emptySlotOtherPos = GetEmptySlotPosition();

                    if(slotToStackToOtherPos != -1)
                    {
                        // Assign quantity to slot
                        int qtdOtherSlot = bagSlot.item.maxStackAmount - slots[slotToStackToOtherPos].quantity;
                        slots[slotToStackToOtherPos].quantity += Mathf.Min(qtdToAdd, qtdOtherSlot);

                        // Item in quick slot?
                        int quickSlotItemOtherPos = IsItemInQuickSlots(slotToStackToOtherPos);
                        if(quickSlotItemOtherPos != -1)
                        {
                            quickSlots[quickSlotItemOtherPos].quantity += Mathf.Min(qtdToAdd, qtdOtherSlot);
                        }

                        // Remove quantity
                        qtdToAdd -= qtdOtherSlot;
                    }
                    else if(emptySlotOtherPos != -1)
                    {
                         // Has empty slot
                        int qtdOtherSlot = Mathf.Min(qtdToAdd, bagSlot.item.maxStackAmount);

                        slots[emptySlotOtherPos].item = bagSlot.item;
                        slots[emptySlotOtherPos].quantity = qtdOtherSlot;

                        // Remove quantity
                        qtdToAdd -= qtdOtherSlot;
                    }

                    // Can't add to inventory
                    if(slotToStackToOtherPos == -1 && emptySlotOtherPos == -1)
                        return;
                }

                // Remove item from bag
                bagSlot.ItemSlotDraggedToInventory();

                if(qtdToAdd > 0)
                {
                    // Add item to dropped bag
                    droppedBag.AddItem(slots[quickSlotTarget.itemSlotPosition].item, qtdToAdd);
                }

                // Update UI Slots
                UIManager.Instance.UpdateInventorySlots(slots);

                // Update Quick UI Slots
                UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);

                return;
            }
        }

        // Has empty slot
        int emptySlotPos = GetEmptySlotPosition();
        if(emptySlotPos == -1)
            return;

        // Assign to empty slot
        slots[emptySlotPos].item = bagSlot.item;
        slots[emptySlotPos].quantity = bagSlot.quantity;

        // If target quick slot position equals selected position
        if(quickSlotSelectedIndex == targetQuickSlotIndex)
        {
            // If item in target slot is equipable
            SelectItem(emptySlotPos, targetQuickSlotIndex);
            
            // New item is equipable
            if(slots[emptySlotPos].item.type == ItemType.Equipable)
                Equip();
        }

        // Assign to quick slot
        quickSlots[targetQuickSlotIndex].item = slots[emptySlotPos].item;
        quickSlots[targetQuickSlotIndex].quantity = slots[emptySlotPos].quantity;
        quickSlots[targetQuickSlotIndex].itemSlotPosition = emptySlotPos;
        
        // Remove item from bag
        bagSlot.ItemSlotDraggedToInventory();

        // Update UI Slots
        UIManager.Instance.UpdateInventorySlots(slots);

        // Update Quick UI Slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
    }

    private void OnMouseClickEvent()
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
            OnConsume(itemSlot);
        }
        else if(itemSlot.item.type == ItemType.Building)
        {
            Build();
        }

    }

    #region Building
    private void Build()
    {
        playerBuild.Build();
    }

    private void UnEquipBuild()
    {
        playerBuild.UnBuild();
    }

    private void StartBuilding()
    {
        playerBuild.SetNewBuilding(slots[selectedItemIndex].item);
    }
    #endregion

    #region Consumables
    private void OnConsume(ItemSlot itemSlot)
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

        ReduceInventoryQuantity();
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
    public void ReduceInventoryQuantity()
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

    private int GetEmptySlotPosition()
    {
        // Search for a empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item == null)
            {
                return i;
            }
        }

        return -1;
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
            if(quickSlots[quickSlotSelectedIndex].item.type == ItemType.Equipable)
            {
                // UnEquip
                UnEquip();
            }

            // If Old selected is building
            if(quickSlots[quickSlotSelectedIndex].item.type == ItemType.Building)
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
            if(quickSlots[quickSlotSelectedIndex].item.type == ItemType.Equipable)
            {
                // UnEquip
                UnEquip();
            }

            // If Old selected is building
            if(quickSlots[quickSlotSelectedIndex].item.type == ItemType.Building)
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
    public event Action<ItemSlot> OnItemSlotDraggedToInventory;
    public void ItemSlotDraggedToInventory(){OnItemSlotDraggedToInventory?.Invoke(this);}
}

[System.Serializable]
public class QuickItemSlot
{
    public ItemData item;
    public int itemSlotPosition;
    public int quantity;
}
