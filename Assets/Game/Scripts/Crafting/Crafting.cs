using UnityEngine;

public class Crafting : Singleton<Crafting>
{
    [Header("Components")]
    [SerializeField] private InputReader inputReader;
    
    [Header("Selected Item")]
    private CraftingRecipeData selectedItem;
    
    private void Awake() 
    {
        inputReader = GetComponent<InputReader>();
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

    public void SelectItem(CraftingRecipeData recipeData)
    {
        selectedItem = recipeData;

        // Update Craft information
        UIManager.Instance.UpdateCraftInfo(recipeData);
    }

    public void OnCraftButton()
    {
        if(selectedItem == null)
        {
            return;
        }

        // Can craft?
        bool canCraft = true;

        for (int i = 0; i < selectedItem.resourceCosts.Length; i++)
        {
            if(!Inventory.Instance.HasItems(selectedItem.resourceCosts[i].item, selectedItem.resourceCosts[i].quantity))
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
            for (int x = 0; x < selectedItem.resourceCosts[i].quantity; x++)
            {
                Inventory.Instance.RemoveItem(selectedItem.resourceCosts[i].item);
            }
        }

        // Add item to inventory
        Inventory.Instance.AddItem(selectedItem.itemToCraft);

        // Update Recipes UIs
        UIManager.Instance.UpdateCraftRecipesUIs();
    }
}
