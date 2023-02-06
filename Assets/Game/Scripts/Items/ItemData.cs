using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "New Item Recipe")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string displayName;
    public string description;
    public ItemType type;
    public Sprite icon;
    public GameObject dropPrefab;

    [Header("Stacking")]
    public bool canStack;
    public int maxStackAmount;

    [Header("Consumable")]
    public ItemDataConsumable[] consumables;

    [Header("Equip")]
    public GameObject equipPrefab;

    [Header("Build")]
    public GameObject spawnPrefab;
    public GameObject previewPrefab;
}

public enum ItemType
{
    Resource,
    Equipable,
    Consumable,
    Building
}

public enum ConsumableType
{
    Hunger,
    Thirst,
    Health
}

[System.Serializable]
public class ItemDataConsumable
{
    public ConsumableType type;
    public float value;
}