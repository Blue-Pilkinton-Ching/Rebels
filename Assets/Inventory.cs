using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int initialSlots = 2;
    public GameObject SlotPrefab;
    public Vector2 SlotOffset;
    public float SlotGap;
    public Inventory Singleton { get; private set; } = null;
    public List<InventorySlot> Slots { get; private set; } = new();
    private void Awake()
    {
        Singleton = this;
        AddCells(initialSlots);
    }

    public void AddSlots(int count)
    {
        AddCells(count);
    }

    private void AddCells(int count)
    {
        for (var i = 0; i < count; i++)
        {
            GameObject slotObject = Instantiate(SlotPrefab, transform);

            slotObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(SlotOffset.x + (SlotGap * Slots.Count), SlotOffset.y);

            Slots.Add(slotObject.GetComponent<InventorySlot>());
        }
    }
}
