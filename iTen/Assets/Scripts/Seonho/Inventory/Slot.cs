using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Slot : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI itemCountText;
    public Image highlightImage;
    public ItemData currentItem;
    private int itemCount;
    public bool HasItem => currentItem != null;
    public int ItemCount => itemCount;

    private void Start()
    {
        UpdateUI();
        SetHighlight(false);
    }

    public void AddItem(ItemData itemData, int amount)
    {
        currentItem = itemData;
        itemCount = amount;
        UpdateUI();
    }

    public void IncreaseItemCount(int amount)
    {
        if (HasItem)
        {
            itemCount += amount;
            UpdateUI();
        }
    }

    public void UseItem()
    {
        if (itemCount > 0)
        {
            itemCount--;
            UpdateUI();

            if (itemCount == 0)
            {
                ClearSlot();
            }
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        itemCount = 0;
        UpdateUI();
    }

    public bool HasSameItem(ItemData itemData) => currentItem != null && currentItem.itemName == itemData.itemName;

    public void Highlight(bool isSelected)
    {
        SetHighlight(isSelected);
    }

    private void SetHighlight(bool isSelected)
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(isSelected);
        }
    }

    private void UpdateUI()
    {
        if (currentItem != null)
        {
            itemImage.sprite = currentItem.icon;
            itemImage.enabled = true;
            itemImage.color = Color.white;

            itemCountText.text = itemCount > 1 ? itemCount.ToString() : "1";
            itemCountText.enabled = true;

        }
        else
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
            itemCountText.text = "";
            itemCountText.enabled = false;
        }
    }
}