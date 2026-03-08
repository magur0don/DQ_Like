using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI CountText;
    public Image IconImage;

    public void SetData(InventryEntry entry)
    {
        NameText.text = entry.Item.ItemName;
        CountText.text = entry.Count.ToString();
        
        if (IconImage != null && entry.Item.Icon != null)
        {
            IconImage.sprite = entry.Item.Icon;
            IconImage.enabled = true;
        }
        else if (IconImage != null)
        {
            IconImage.enabled = false;
        }
    }
}
