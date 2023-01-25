using UnityEngine;

[CreateAssetMenu(fileName = "Crafting Recipe", menuName = "New Crafting Recipe")]
public class CraftingRecipeData : ScriptableObject
{
    public ItemData itemToCraft;
    public ResourceCost[] resourceCosts;
}

[System.Serializable]
public class ResourceCost
{
    public ItemData item;
    public int quantity;
}
