using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Slots")]
    public InventorySlot[] slots;

    [Header("References")]
    public HUDManager hudManager;

    void Start()
    {
        // Inicjalizacja slotów
        foreach (InventorySlot slot in slots)
        {
            slot.Initialize();
        }
    }

    public void AddItem(Item item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(item);
                return;
            }
        }
        Debug.Log("Ekwipunek pe³ny!");
    }

    public void RemoveItem(Item item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty() && slot.GetItem() == item)
            {
                slot.ClearSlot();
                return;
            }
        }
    }
}