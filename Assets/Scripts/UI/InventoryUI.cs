using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public GameObject Panel;
    public Transform ItemContainer;
    public GameObject ItemSlotPrefab;

    private void Awake()
    {
        Instance = this;
        Panel.SetActive(false);
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

        // Spawn new slots
        var items = InventoryManager.Instance.GetAll();
        foreach (var entry in items)
        {
            GameObject slotObj = Instantiate(ItemSlotPrefab, ItemContainer);
            InventoryItemSlot slot = slotObj.GetComponent<InventoryItemSlot>();
            if (slot != null)
            {
                slot.SetData(entry);
            }
        }
    }
}
