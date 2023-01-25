using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingRecipeUI : MonoBehaviour
{
    [SerializeField] private CraftingRecipeData recipeData;
    [SerializeField] private Image itemIcon;

    [SerializeField] private Color canCraftColor;
    [SerializeField] private Color cannotCraftColor;

    private Outline outline;
    private bool canCraft;

    private void Awake() 
    {
        outline = GetComponent<Outline>();
    }

    private void Start() 
    {
        itemIcon.sprite = recipeData.itemToCraft.icon;
    }

    private void OnEnable() 
    {
        UpdateCanCraft();
    }

    public void UpdateCanCraft()
    {
        canCraft = true;

        for (int i = 0; i < recipeData.resourceCosts.Length; i++)
        {
            if(!Inventory.Instance.HasItems(recipeData.resourceCosts[i].item, recipeData.resourceCosts[i].quantity))
            {
                canCraft = false;
                break;
            }
        }

        outline.effectColor = canCraft ? canCraftColor : cannotCraftColor;
    }

    public void OnClickButton()
    {
        Crafting.Instance.SelectItem(recipeData);
    }
}
