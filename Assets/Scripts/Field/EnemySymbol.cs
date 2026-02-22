using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySymbol : MonoBehaviour
{
    private string BattleSceneName = "BattleScene";

    /// <summary>
    /// 侵入判定でPlayerが入ってきたときに処理を行う
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        // PlayerのTag以外のGameObjectが侵入してきたら何もしない
        if (!other.CompareTag("Player"))
        {
            return;
        }
        SceneManager.LoadScene(BattleSceneName);
    }
}
