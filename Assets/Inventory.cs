using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 3;
    public GameObject SlotPrefab;
    public Vector2 SlotOffset;
    public float SlotGap;
    public Inventory Singleton { get; private set; } = null;
    public InventorySlot[] Slots { get; private set; }

    private void Awake()
    {
        Slots = new InventorySlot[maxSlots];

        for (var i = 0; i < maxSlots; i++)
        {
            GameObject slotObject = Instantiate(SlotPrefab, new Vector3(SlotOffset.x + (SlotGap * i), SlotOffset.y), Quaternion.identity, transform);
            Slots[i] = slotObject.GetComponent<InventorySlot>();
        }
        Singleton = this;
    }
}
