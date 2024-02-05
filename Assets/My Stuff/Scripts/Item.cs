using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Rebels/Item", order = 0)]
public class Item : ScriptableObject
{
    public string Name;
    public Sprite Sprite;
    public ItemType itemType;
}

public enum ItemType
{
    Weapon
}