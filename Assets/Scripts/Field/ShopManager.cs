using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("ショップで売っているアイテムと装備")]
    public List<ItemData> ShopItems;
    public List<EquipmentData> ShopEquipments;

    public void BuyItem(ItemData itemToBuy)
    {
        if (itemToBuy == null)
        {
            return;
        }
        if (PlayerState.Instance.ConsumeGold(itemToBuy.Price))
        {
            // ゴールドが減額されたので、インベントリにアイテムを入れる！
            InventoryManager.Instance.Add(itemToBuy, 1);
            DialogUI.Instance.ShowSimpleMessage($"{itemToBuy.ItemName} を 1個 買った！");
        }
        else
        {
            DialogUI.Instance.ShowSimpleMessage("おかねが たりない！");
        }
        // なぜかインスペクター等で非表示、ShopのCanvasを閉じる想定
        this.gameObject.SetActive(false);
    }

    public void BuyEquipment(EquipmentData equipmentToBuy)
    {
        if (equipmentToBuy == null)
        {
            return;
        }
        if (PlayerState.Instance.ConsumeGold(equipmentToBuy.Price))
        {
            EquipmentManager.Instance.Equip(equipmentToBuy);
            // ScriptableObjectのnameを使っています
            DialogUI.Instance.ShowSimpleMessage($"{equipmentToBuy.DisplayName} を そうびした！");
        }
        else
        {
            DialogUI.Instance.ShowSimpleMessage("おかねが たりない！");
        }
        this.gameObject.SetActive(false);
    }
}
