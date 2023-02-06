using UnityEngine;

[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(Inventory))]
public class Crafting : Singleton<Crafting>
{
    [Header("Components")]
    private InputReader inputReader;
    private Inventory inventory;
    
    [Header("Selected Item")]
    private CraftingData selectedItem;
    
    private void Awake() 
    {
        inputReader = GetComponent<InputReader>();
        inventory = GetComponent<Inventory>();
    }

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

    private void OnInventory()
    {
        // Called onClick Tab to inventory
        if(!UIManager.Instance.IsInventoryOpen())
        {
            ClearSelectedItemWindow();
        }
    }

    private void ClearSelectedItemWindow()
    {
        // Clear selected item
        selectedItem = null;

        // Disable craft item information
        UIManager.Instance.UpdateCraftInfo();
    }

    public void SelectItem(CraftingData recipeData)
    {
        selectedItem = recipeData;

        // Update Craft information
        UIManager.Instance.UpdateCraftInfo(recipeData);
    }

    public void OnCraftButton()
    {
        // Has selected item
        if(selectedItem == null)
            return;

        // Can craft?
        bool canCraft = true;

        for (int i = 0; i < selectedItem.resourceCosts.Length; i++)
        {
            if(!inventory.HasItems(selectedItem.resourceCosts[i].item, selectedItem.resourceCosts[i].quantity))
            {
                canCraft = false;
                break;
            }
        }

        // If can craft, craft
        if(canCraft)
        {
            Craft();
        }
    }

    private void Craft()
    {
        // Remove resources costs from inventory
        for (int i = 0; i < selectedItem.resourceCosts.Length; i++)
        {
            inventory.RemoveResourcesCosts(selectedItem.resourceCosts[i].item, selectedItem.resourceCosts[i].quantity);
        }

        // Add item to inventory
        inventory.AddItem(selectedItem.itemToCraft);

        // Update Recipes UIs
        UIManager.Instance.UpdateCraftRecipesUIs();
    }
}
