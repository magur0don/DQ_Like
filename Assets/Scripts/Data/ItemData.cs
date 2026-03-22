using UnityEngine;

[CreateAssetMenu(menuName = "DQ-Like/Items/ItemData",
    fileName = "item_")]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        HealHP,
        // 将来拡張
        HealMP,
        BuffAttack,
        Revive
    }

    public string ItemName;

    public ItemType Type;

    public bool CanUseInBattle = true;

    [TextArea(2,4)]
    public string Description;

    // 任意ですが、UIで使うアイコン
    public Sprite Icon;

    [Header("効果の値")]
    public float Power;   // 回復量など

    [Header("Shop用のデータ")]
    public int Price; // 値段
}
