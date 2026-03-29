using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySymbol : MonoBehaviour
{
    private string BattleSceneName = "BattleScene";

    public int[] NextEnemyIDs;

    private Transform playerTransform;

    private void Start()
    {
        // Playerタグがついているオブジェクトを探して変数に入れておく
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            // プレイヤーの方向を計算
            Vector3 diff = playerTransform.position - transform.position;
            diff.y = 0; // 高さは無視してY軸のみを回転させる

            if (diff != Vector3.zero)
            {
                // Quaternion.Slerpを利用して滑らかに振り向かせる
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime * 5f);
            }
        }
    }

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
        BattleManager.NextEnemyIDs = NextEnemyIDs;
        SceneManager.LoadScene(BattleSceneName);
    }
}
