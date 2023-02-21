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

        EventBus.Instance.Subscribe("SwapItems", SwapItems);
        EventBus.Instance.Subscribe("AddToQuickSlot", AddToQuickSlot);
    }

    private void OnDisable() 
    {
        // Unsubscribe to events
        inputReader.InventoryEvent -= OnInventory;
        inputReader.QuickSlotClick -= OnQuickSlotClick;
        inputReader.MouseClickEvent -= OnMouseClickEvent;

        EventBus.Instance.Unsubscribe("SwapItems", SwapItems);
        EventBus.Instance.Unsubscribe("AddToQuickSlot", AddToQuickSlot);
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
        UIManager.Instance.UnsubscribeSlotsEvents();
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

            // Toggle Bag UI
            StorageUI.Instance.Toogle();
            StorageUI.Instance.SetIsInteractingWithBag();
        }
        else
        {
            // Open inventory
            UIManager.Instance.ToggleInventoryWindow(true);

            // Toggle cursor to true
            playerController.ToggleCursor(true);

            // Toggle IsInteraction to true
            playerController.ToggleInteract(true);
        }
    }

    private void OnMouseClickEvent()
    {
        if(selectedItemIndex == -1)
            return;

        // Has selected item ? 
        ItemSlot itemSlot = slots[selectedItemIndex];

        if(itemSlot.Item == null || playerController.IsInteracting)
            return;

        // Is consumable type
        if(itemSlot.Item.type == ItemType.Consumable)
        {
            OnConsume(itemSlot);
        }
        else if(itemSlot.Item.type == ItemType.Building)
        {
            Build();
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

    public void SwapItems(object data)
    {
        // Swap items in inventory
        
        SwapItemsModel eventData = data as SwapItemsModel;

        int toSwapIndex = eventData.targetPosition;
        int swapIndex = eventData.draggablePosition;

        // Is index valid
        if(slots[toSwapIndex] == null || slots[swapIndex] == null)
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
    
    public void DropItem(int index)
    {
        // Is index valid
        if(slots[index] == null || slots[index].Item == null)
            return;
        
        // TODO:: Improve Se sair da zona do dropped bag chamar evento para colocar o dropped ba a null
        // Drop a bag
        if(droppedBag == null)
        {
            droppedBag = Instantiate(dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * UnityEngine.Random.value * 360.0f)).GetComponent<DroppedBag>();
        }

        if(droppedBag.CanAddItem())
        {
            // Add item to dropped bag
            droppedBag.AddItem(slots[index].Item, slots[index].Quantity);

            // Remove item from inventory
            RemoveItem(index);
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
    #endregion

    /* 
    public void AddItemFromBag(int toSwapIndex, ItemSlot bagSlot)
    {
        // deviamos colocar a vir o objeto que estamos a interagir pois pode ser um safe ou o drop bag e podemos ter de descontar lá a quantidade

        Debug.Log("Entrou");
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
    } */

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
        playerBuild.SetNewBuilding(slots[selectedItemIndex].Item);
    }
    #endregion

    #region Consumables
    private void OnConsume(ItemSlot itemSlot)
    {   
        for (int i = 0; i < itemSlot.Item.consumables.Length; i++)
        {
            switch (itemSlot.Item.consumables[i].type)
            {
                case ConsumableType.Health: 
                    playerNeeds.Heal(itemSlot.Item.consumables[i].value);
                    break;
                
                case ConsumableType.Hunger: 
                    playerNeeds.Eat(itemSlot.Item.consumables[i].value);
                    break;
                
                case ConsumableType.Thirst: 
                    playerNeeds.Drink(itemSlot.Item.consumables[i].value);
                    break;

                default:
                    return;
            }
        }

        OnActionReduceSelectedItemQuantity();
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

    public void OnActionReduceSelectedItemQuantity()
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
        }

        // Update UI Slots
        UIManager.Instance.UpdateInventorySlots(slots);

        // Update Quick Slots
        UIManager.Instance.UpdateInventoryQuickSlots(quickSlots);
    } 

    #endregion

    #region Quick Slots
    public void AddToQuickSlot(object data) 
    {
        SwapItemsModel eventData = data as SwapItemsModel;

        int targetQuickSlotIndex = eventData.targetPosition;
        int inventoryItemIndex = eventData.draggablePosition;

        // Is index valid
        if(quickSlots[targetQuickSlotIndex] == null || slots[inventoryItemIndex] == null)
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
    #endregion
}

public class ItemSlot
{
    private ItemData itemData;
    private int quantity;

    public ItemData Item => itemData;
    public int Quantity => quantity;

    public ItemSlot(ItemData data, int amount)
    {
        itemData = data;
        quantity = amount;
    }

    public ItemSlot()
    {
        ClearSlot();
    }

    public virtual void ClearSlot()
    {
        itemData = null;
        quantity = -1;
    }

    public void UpdateSlot(ItemData data, int amount)
    {
        itemData = data;
        quantity = amount;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;

        if(quantity > itemData.maxStackAmount)
            quantity = itemData.maxStackAmount;
    }

    public void RemoveQuantity(int amount)
    {
        quantity -= amount;

        if(quantity < 0)
            quantity = 0;
    }

    public bool RoomLeftInStack(int amountToAdd, out int amountRemaining)
    {
        amountRemaining = itemData.maxStackAmount - quantity;
        
        return RoomLeftInStack(amountToAdd);
    }

    public bool RoomLeftInStack(int amountToAdd)
    {
        if(quantity + amountToAdd <= itemData.maxStackAmount) return true;
        else return false;
    }

    /* public event Action<ItemSlot> OnItemSlotDraggedToInventory;
    public void ItemSlotDraggedToInventory(){OnItemSlotDraggedToInventory?.Invoke(this);}

    public delegate void DelegateSwap(ItemSlot swapItem, ItemSlot toSwapItem);
    public event DelegateSwap OnItemSwapBag;
    public void ItemSlotSwappedInBag(ItemSlot swapItem)
    {
        if(OnItemSwapBag == null)
            return;            

        OnItemSwapBag(swapItem, this);
    }

    public delegate void DelegateDrop(ItemSlot swapItem, ItemSlot toSwapItem);
    public event DelegateDrop OnItemDropBag;
    public void ItemSlotDropInBag(ItemSlot swapItem)
    {
        if(OnItemSwapBag == null)
            return;            

        OnItemDropBag(swapItem, this);
    } */
}

public class QuickItemSlot : ItemSlot
{
    private int itemSlotPosition;
    public int ItemSlotPosition => itemSlotPosition;

    public override void ClearSlot()
    {
        base.ClearSlot();

        itemSlotPosition = -1;
    }

    public void UpdateQuickSlot(ItemData data, int amount, int slotPosition)
    {
        UpdateSlot(data, amount);

        itemSlotPosition = slotPosition;
    }

    public void SetItemSlotPosition(int slotPosition)
    {
        itemSlotPosition = slotPosition;
    }
}

public class SwapItemsModel
{
    public int targetPosition;
    public int draggablePosition;

    public SwapItemsModel(int tPos, int dPos)
    {
        targetPosition = tPos;
        draggablePosition = dPos;
    }
}