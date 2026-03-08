using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI CountText;
    public Image IconImage;
    public Image HighlightImage;
    public Button SelectButton;

    private InventryEntry entry;

    public void SetData(InventryEntry entry)
    {
        this.entry = entry;
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

        if (HighlightImage != null) HighlightImage.enabled = false;

        if(SelectButton ==null)return;

        SelectButton.onClick.RemoveAllListeners();
        SelectButton.onClick.AddListener(() => {
            InventoryUI.Instance.SelectItem(entry);
        });
    }

    public void SetSelected(bool isSelected)
    {
        if (HighlightImage != null)
        {
            HighlightImage.enabled = isSelected;
        }
    }
}
