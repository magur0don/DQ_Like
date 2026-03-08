using UnityEngine;
using TMPro;

public class EnemyStatusUI : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI HPText;

    private EnemyBattleInfo targetEnemy;

    public void Setup(EnemyBattleInfo enemy)
    {
        targetEnemy = enemy;
        Refresh();
    }

    public void Refresh()
    {
        if (targetEnemy == null) return;

        NameText.text = targetEnemy.Data.DisplayName;
        HPText.text = $"HP:{(int)targetEnemy.CurrentHP}/{(int)targetEnemy.Data.MaxHP}";

        // 死亡時は表示を薄くするなどの演出も可能
        if (targetEnemy.IsDead)
        {
            NameText.color = Color.gray;
            HPText.color = Color.gray;
        }
    }
}
