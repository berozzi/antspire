using static UnityEditor.Progress;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InventorySlot
{
    public Image itemIcon;
    public Text itemCountText;
    private Item currentItem;
    private int itemCount;

    public void Initialize()
    {
        ClearSlot();
    }

    public void SetItem(Item item)
    {
        currentItem = item;
        itemCount = 1;
        //itemIcon.sprite = item.
        itemIcon.enabled = true;
        UpdateCountText();
    }

    public void ClearSlot()
    {
        currentItem = null;
        itemCount = 0;
        itemIcon.enabled = false;
        itemCountText.text = "";
    }

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public Item GetItem()
    {
        return currentItem;
    }

    private void UpdateCountText()
    {
        if (itemCount > 1)
        {
            itemCountText.text = itemCount.ToString();
        }
        else
        {
            itemCountText.text = "";
        }
    }
}