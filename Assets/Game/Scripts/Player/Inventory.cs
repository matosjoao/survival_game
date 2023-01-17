using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class Inventory : Singleton<Inventory>
{


    [Header("Components")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private PlayerController controller;
    [SerializeField] private PlayerNeeds playerNeeds;
    [SerializeField] private PlayerCamera playerCamera;

    [Header("Properties")]
    [SerializeField] private ItemSlotUI[] uiSlots;
    [SerializeField] private GameObject inventoryWindow;
    [SerializeField] private Transform dropPosition;

    [Header("Properties UI")]
    [SerializeField] private GameObject useButton;
    [SerializeField] private GameObject equipButton;
    [SerializeField] private GameObject unEquipButton;
    [SerializeField] private GameObject dropButton;

    [Header("Selected Item")]
    private ItemSlot selectedItem;
    private int selectedItemIndex;
    
    [Header("Events")]
    public UnityEvent onOpenInventory;
    public UnityEvent onCloseInventory;

    private ItemSlot[] slots;
    private int curEquipIndex;



    private void OnEnable() 
    {
        inputReader.InventoryEvent += OnInventory;
    }

    private void OnDisable() 
    {
        inputReader.InventoryEvent -= OnInventory;
    }

    private void Start() 
    {
        inventoryWindow.SetActive(false);
        
        // Initialize Slots
        slots = new ItemSlot[uiSlots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemSlot();
            uiSlots[i].index = i;
            uiSlots[i].Clear();
        }

        ClearSelectedItemWindow();
    }

    private void OnInventory()
    {
        if(inventoryWindow.activeInHierarchy)
        {
            inventoryWindow.SetActive(false);
            onCloseInventory.Invoke();
            playerCamera.ToggleCursor(false);
        }
        else
        {
            playerCamera.ToggleCursor(true);
            inventoryWindow.SetActive(true);
            onOpenInventory.Invoke();
            ClearSelectedItemWindow();
        }
    }

    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy;
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
                UpdateUI();
                return;
            }
        }

        // Has empty slot
        ItemSlot emptySlot = GetEmptySlot();
        if(emptySlot != null)
        {
            emptySlot.item = item;
            emptySlot.quantity = 1;
            UpdateUI();
            return;
        }

        // Can't stack and no empty slot
        ThrowItem(item);
    }

    private void ThrowItem(ItemData item)
    {
        Instantiate(item.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360.0f));
    }

    private void UpdateUI()
    {
        // Update UI Slots
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item != null)
            {
                uiSlots[i].Set(slots[i]);
            }
            else
            {
                uiSlots[i].Clear();
            }
        }
    }

    private ItemSlot GetItemStack(ItemData item)
    {
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
        if(slots[index].item == null)
        {
            return;
        }

        selectedItem = slots[index];
        selectedItemIndex = index;

        useButton.SetActive(selectedItem.item.type == ItemType.Consumable);
        equipButton.SetActive(selectedItem.item.type == ItemType.Equipable && !uiSlots[index].equipped);
        unEquipButton.SetActive(selectedItem.item.type == ItemType.Equipable && uiSlots[index].equipped);
        dropButton.SetActive(true);
    }

    private void ClearSelectedItemWindow()
    {
        // Clear selected item
        selectedItem = null;
        selectedItemIndex = -1;

        // Disable buttons
        useButton.SetActive(false);
        equipButton.SetActive(false);
        unEquipButton.SetActive(false);
        dropButton.SetActive(false);
    }

    public void OnUseButton()
    {
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

    }

    private void UnEquip(int index)
    {

    }

    public void OnUnEquipButton()
    {

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
            if(uiSlots[selectedItemIndex].equipped == true)
            {
                UnEquip(selectedItemIndex);
            }

            selectedItem.item = null;
            ClearSelectedItemWindow();
        }

        UpdateUI();
    }

    private void RemoveItem(ItemData item)
    {
        
    }

    public bool HasItems(ItemData item, int quantity)
    {
        return false;
    }
}

public class ItemSlot
{
    public ItemData item;
    public int quantity;
}
