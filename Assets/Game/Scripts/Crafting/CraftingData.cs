using UnityEngine;

[CreateAssetMenu(fileName = "Crafting", menuName = "New Crafting Recipe")]
public class CraftingData : ScriptableObject
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
