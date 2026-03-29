using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public GameObject Panel;
    public Transform ItemContainer;
    public GameObject ItemSlotPrefab;

    [Header("Details UI")]
    public TextMeshProUGUI DetailNameText;
    public TextMeshProUGUI DetailDescriptionText;
    public Button UseButton;

    private InventryEntry selectedEntry;
    private List<InventoryItemSlot> slots = new List<InventoryItemSlot>();

    private void Awake()
    {
        Instance = this;
        Panel.SetActive(false);
        if (UseButton != null) UseButton.onClick.AddListener(OnUseButtonClicked);
    }

    public void Toggle()
    {
        if (Panel.activeSelf)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        GameState.IsInventoryOpen = true;
        Panel.SetActive(true);
        selectedEntry = null;
        UpdateDetails();
        Refresh();
    }

    public void Close()
    {
        GameState.IsInventoryOpen = false;
        Panel.SetActive(false);
    }

    public void Refresh()
    {
        // Clear existing slots
        foreach (Transform child in ItemContainer)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();

        // Spawn new slots
        var items = InventoryManager.Instance.GetAll();
        foreach (var entry in items)
        {
            GameObject slotObj = Instantiate(ItemSlotPrefab, ItemContainer);
            InventoryItemSlot slot = slotObj.GetComponent<InventoryItemSlot>();
            if (slot != null)
            {
                slot.SetData(entry);
                slots.Add(slot);
            }
        }
    }

    public void SelectItem(InventryEntry entry)
    {
        selectedEntry = entry;
        UpdateDetails();

        // Highlight the selected slot
        foreach (var slot in slots)
        {
            // entryが一致するかどうかで判定
            // (本来はIDなどがあると良いですが、ここでは参照で比較します)
            slot.SetSelected(false);
        }

        // 選択されたスロットをハイライト
        var selectedSlot = slots.Find(s => s.gameObject.activeInHierarchy && s.NameText.text == entry.Item.ItemName);
        if (selectedSlot != null) selectedSlot.SetSelected(true);
    }

    private void UpdateDetails()
    {
        if (selectedEntry == null)
        {
            if (DetailNameText != null) DetailNameText.text = "----";
            if (DetailDescriptionText != null) DetailDescriptionText.text = "アイテムを選択してください";
            if (UseButton != null) UseButton.interactable = false;
            return;
        }

        if (DetailNameText != null) DetailNameText.text = selectedEntry.Item.ItemName;
        if (DetailDescriptionText != null) DetailDescriptionText.text = selectedEntry.Item.Description;
        if (UseButton != null) UseButton.interactable = selectedEntry.Count > 0;
    }

    private void OnUseButtonClicked()
    {
        if (selectedEntry == null || selectedEntry.Count <= 0) return;

        // アイテムを使用するロジック
        // ここではInventoryManagerのUseItemを呼び出します
        if (InventoryManager.Instance.UseItem(selectedEntry.Item))
        {
            Debug.Log($"{selectedEntry.Item.ItemName} を使用しました");
            
            // 効果の適用（将来的に拡張可能）
            ApplyItemEffect(selectedEntry.Item);

            if (selectedEntry.Count <= 0)
            {
                selectedEntry = null;
            }

            // 表示の更新
            UpdateDetails();
            Refresh();
        }
    }

    private void ApplyItemEffect(ItemData item)
    {
        if (item.Type == ItemData.ItemType.HealHP)
        {
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.CurrentHP += item.Power;
                if (PlayerState.Instance.CurrentHP > PlayerState.Instance.MaxHP)
                {
                    PlayerState.Instance.CurrentHP = PlayerState.Instance.MaxHP;
                }
                Debug.Log($"{item.ItemName}を使用しました。HPが回復しました。現在のHP:{PlayerState.Instance.CurrentHP}");
            }
        }
        else if (item.Type == ItemData.ItemType.HealMP)
        {
            Debug.Log($"{item.ItemName}を使用しました。MPが回復しました。");
        }
    }
}
