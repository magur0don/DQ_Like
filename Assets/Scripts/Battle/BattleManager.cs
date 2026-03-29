using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public enum BattleMenuState
{
    Root,   // たたかう/さくせん/にげる
    Fight,  // こうげき/じゅもん/とくぎ/ぼうぎょ
    Busy    // 演出中(入力不可)
}


public class BattleManager : MonoBehaviour
{
    public static int[] NextEnemyIDs = new int[] { 0, 0 };

    [Header("EnemyData")]
    public EnemyDatabase EnemyDB;
    private System.Collections.Generic.List<EnemyBattleInfo> enemies = new System.Collections.Generic.List<EnemyBattleInfo>();

    [Header("Enemy Visual")]
    public Transform EnemyModelRoot;


    [Header("PlayerStatusとLevelSystemの参照")]
    public PlayerStatus PlayerStatus;
    public LevelSystem LevelSystem;

    [Header("PlayerData")]
    public float PlayerMaxHP = 30f;
    public float PlayerHP = 30;
    public float PlayerAttackMin = 5;
    public float PlayerAttackMax = 10;



    [Header("Enemy HP")]
    public float EnemyHP;

    [Header("UI")]
    public TextMeshProUGUI PlayerHPText;
    public TextMeshProUGUI DialogText;

    [Header("Enemy Status UI")]
    public EnemyStatusUI EnemyStatusUIPrefab;
    public Transform EnemyStatusUIRoot;
    private System.Collections.Generic.List<EnemyStatusUI> enemyStatusUIs = new System.Collections.Generic.List<EnemyStatusUI>();

    [Header("DQ Like Menu")]
    public GameObject RootMenuPanel;
    public Transform RootMenuRoot;
    public GameObject FightMenuPanel;
    public Transform FightMenuRoot;

    public MenuButton MenuButtonPrefab;

    private BattleMenuState menuState =
        BattleMenuState.Root;
    private bool isGuading = false;




    private bool isPlayerTurn = true;

    [Header("BGM Settings")]
    public AudioClip RegularBGM;
    public AudioClip BossBGM;

    void Start()
    {
        SetupEnemiesFromDB();
        ApplyPlayerStatus();

        UpdateUI();

        BuildRootMenu();

        // 戦闘の開始時に生成も行う
        SpawnEnemyModels();

        if (enemies.Count > 1)
        {
            DialogText.text = "まものたちが あらわれた！";
        }
        else if (enemies.Count == 1)
        {
            DialogText.text = $"{enemies[0].Data.DisplayName} が現れた！";
        }

        PlayBattleBGM();
    }

    private void PlayBattleBGM()
    {
        if (BGMManager.Instance == null) return;

        bool isBossBattle = false;
        foreach (var enemy in enemies)
        {
            if (enemy.Data.IsBoss)
            {
                isBossBattle = true;
                break;
            }
        }

        AudioClip bgmToPlay = isBossBattle ? BossBGM : RegularBGM;
        if (bgmToPlay != null)
        {
            BGMManager.Instance.PlayBGM(bgmToPlay);
        }
    }

    /// データからプレイヤーの値を反映する
    public void ApplyPlayerStatus()
    {
        if (PlayerStatus == null)
        {
            return;
        }
        PlayerMaxHP = PlayerState.Instance.MaxHP;
        PlayerHP = Mathf.Min(PlayerState.Instance.CurrentHP, PlayerMaxHP);
        PlayerAttackMin = PlayerState.Instance.AttackMin;
        PlayerAttackMax = PlayerState.Instance.AttackMax;
        Debug.Log($"{PlayerAttackMax}");
    }



    private void SetupEnemiesFromDB()
    {
        if (EnemyDB == null)
        {
            Debug.LogError("EnemyDB設定されてません");
            return;
        }

        enemies.Clear();
        foreach (var id in NextEnemyIDs)
        {
            var data = EnemyDB.GetByID(id);
            if (data != null)
            {
                enemies.Add(new EnemyBattleInfo(data));
            }
        }

        if (enemies.Count == 0)
        {
            Debug.LogError("敵が1体もみつかりませんでした");
        }
    }

    /// <summary>
    /// 敵のVisualを生成
    /// </summary>
    private void SpawnEnemyModels()
    {
        if (EnemyModelRoot == null)
        {
            return;
        }

        // 既存のモデルを削除
        ClearChildren(EnemyModelRoot);

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy.Data.ModelPrefab == null) continue;

            // Instantiateを使って、敵のモデルを、EnemyModelRootに生成
            GameObject instance = Instantiate(enemy.Data.ModelPrefab, EnemyModelRoot);
            enemy.ModelInstance = instance;

            // 敵の位置情報を設定 (複数体の場合は横に並べるなどの調整が必要)
            // 仮でX座標をずらす
            float xOffset = (enemies.Count > 1) ? (i - (enemies.Count - 1) * 0.5f) * 2.0f : 0;
            Vector3 pos = enemy.Data.ModelPosition;
            pos.x += xOffset;

            instance.transform.localPosition = pos;
            instance.transform.localEulerAngles = enemy.Data.ModelRotation;
            instance.transform.localScale = enemy.Data.ModelScale;

            // EnemyAnimatorを取得（なければ追加）
            enemy.Animator = instance.GetComponent<EnemyAnimator>();
            if (enemy.Animator == null)
            {
                enemy.Animator = instance.AddComponent<EnemyAnimator>();
            }
        }
    }

    private void SetMenuState(BattleMenuState state)
    {
        menuState = state;
        // Rootのメニューパネルの表示
        if (RootMenuPanel != null)
        {
            RootMenuPanel.SetActive(
                state == BattleMenuState.Root);
        }
        // 戦闘パネルの表示
        if (FightMenuPanel != null)
        {
            FightMenuPanel.SetActive(
                state == BattleMenuState.Fight);
        }

        if (state == BattleMenuState.Busy)
        {
            if (RootMenuPanel != null)
            {
                RootMenuPanel.SetActive(false);
            }
            if (FightMenuPanel != null)
            {
                FightMenuPanel.SetActive(false);
            }
        }
    }

    private void BuildRootMenu()
    {
        // RootMenuの子の階層にあるgameObjectを削除します
        ClearChildren(RootMenuRoot);

        CreateButton(RootMenuRoot, "たたかう", () =>
        {
            if (!isPlayerTurn)
            {
                return;
            }

            // たたかうメニューを設定します
            BuildFightMenu();
            SetMenuState(BattleMenuState.Fight);
            DialogText.text = "どうする？";
        });

        CreateButton(RootMenuRoot, "アイテム", () =>
        {
            if (!isPlayerTurn)
            {
                return;
            }

            // アイテム選択メニューを表示
            BuildItemMenu();
        });

        CreateButton(RootMenuRoot, "さくせん", () =>
        {
            if (!isPlayerTurn)
            {
                return;
            }
            DialogText.text = "さくせんは　まだ　つかえない！";
        });
        CreateButton(RootMenuRoot, "にげる", () =>
        {

            if (!isPlayerTurn)
            {
                return;
            }
            StartCoroutine(TryEscape());
        });
    }
    private void BuildFightMenu()
    {
        ClearChildren(FightMenuRoot);
        CreateButton(FightMenuRoot, "こうげき", () =>
        {
            if (!isPlayerTurn)
            {
                return;
            }
            // ターゲット選択メニューを表示
            BuildTargetMenu((target) =>
            {
                StartCoroutine(ExecuteAttack(target));
            });
        });
        CreateButton(FightMenuRoot, "じゅもん", () =>
        {
            if (!isPlayerTurn)
            {
                return;
            }
            StartCoroutine(ExecuteHealSpell());
        });
        CreateButton(FightMenuRoot, "とくぎ", () =>
        {
            if (!isPlayerTurn)
            {
                return;
            }
            // ターゲット選択メニューを表示
            BuildTargetMenu((target) =>
            {
                StartCoroutine(ExecutePowerSkill(target));
            });
        });
        CreateButton(FightMenuRoot, "ぼうぎょ", () =>
        {
            if (!isPlayerTurn)
            {
                return;
            }
            StartCoroutine(ExecuteGuard());
        });
        CreateButton(FightMenuRoot, "もどる", () =>
        {
            SetMenuState(BattleMenuState.Root);
            DialogText.text = "どうする？";
        });
    }

    private void BuildTargetMenu(System.Action<EnemyBattleInfo> onSelected)
    {
        ClearChildren(FightMenuRoot);
        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;

            CreateButton(FightMenuRoot, enemy.Data.DisplayName, () =>
            {
                onSelected?.Invoke(enemy);
            });
        }
        CreateButton(FightMenuRoot, "もどる", () =>
        {
            BuildFightMenu();
        });
    }

    private void BuildItemMenu()
    {
        // アイテムはFightMenuの場所を借りて表示します
        SetMenuState(BattleMenuState.Fight);
        ClearChildren(FightMenuRoot);

        var inventoryItems = InventoryManager.Instance.GetAll();
        bool hasAnyItem = false;

        foreach (var entry in inventoryItems)
        {
            if (entry.Count <= 0 || !entry.Item.CanUseInBattle) continue;

            hasAnyItem = true;
            string label = $"{entry.Item.ItemName} ({entry.Count})";
            CreateButton(FightMenuRoot, label, () =>
            {
                // アイテムを使用
                if (InventoryManager.Instance.UseItem(entry.Item))
                {
                    StartCoroutine(ExecuteUseItem(entry.Item));
                }
            });
        }

        if (!hasAnyItem)
        {
            DialogText.text = "アイテムを　もっていない！";
        }

        CreateButton(FightMenuRoot, "もどる", () =>
        {
            SetMenuState(BattleMenuState.Root);
            DialogText.text = "どうする？";
        });
    }

    // こうげきの処理
    private System.Collections.IEnumerator ExecuteAttack(EnemyBattleInfo target)
    {
        isPlayerTurn = false;
        SetMenuState(BattleMenuState.Busy);

        DialogText.text = $"{target.Data.DisplayName} に こうげき！";
        yield return new WaitForSeconds(0.5f);
        // ダメージ計算で小数点切り上げ
        var damage =
            Mathf.Ceil(
                Random.Range(PlayerAttackMin, PlayerAttackMax)
                );
        target.CurrentHP -= damage;
        DialogText.text = $"{damage} のダメージ！";
        UpdateUI();
        yield return new WaitForSeconds(0.8f);

        if (target.IsDead)
        {
            DialogText.text = $"{target.Data.DisplayName} を たおした！";
            if (target.Animator != null) target.Animator.PlayDie();
            yield return new WaitForSeconds(0.8f);
        }

        if (CheckVictory())
        {
            StartCoroutine(Victory());
            yield break;
        }
        StartCoroutine(EnemyTurn());
    }

    // じゅもんの処理
    private System.Collections.IEnumerator ExecuteHealSpell()
    {
        isPlayerTurn = false;
        SetMenuState(BattleMenuState.Busy);
        DialogText.text = "キュア！";
        yield return new WaitForSeconds(0.6f);

        float heal = Mathf.CeilToInt(PlayerMaxHP * 0.25f) + 2;
        // mathf.Min(A,B)でどちらか小さいほうの値を取得できる
        PlayerHP = Mathf.Min(PlayerMaxHP, PlayerHP + heal);

        DialogText.text = $"{heal} かいふく！";
        UpdateUI();
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(EnemyTurn());
    }

    // とくぎのしょり
    private System.Collections.IEnumerator ExecutePowerSkill(EnemyBattleInfo target)
    {
        isPlayerTurn = false;
        SetMenuState(BattleMenuState.Busy);
        DialogText.text = "つよく きりつけた！";
        yield return new WaitForSeconds(0.6f);
        // ダメージ計算で小数点切り上げ
        var damage =
            Mathf.Ceil(
                Random.Range(PlayerAttackMin, PlayerAttackMax) * 1.6f + 2
                );
        target.CurrentHP -= damage;
        DialogText.text = $"{damage} のダメージ！";
        UpdateUI();
        yield return new WaitForSeconds(0.8f);

        if (target.IsDead)
        {
            DialogText.text = $"{target.Data.DisplayName} を たおした！";
            if (target.Animator != null) target.Animator.PlayDie();
            yield return new WaitForSeconds(0.8f);
        }

        if (CheckVictory())
        {
            StartCoroutine(Victory());
            yield break;
        }
        StartCoroutine(EnemyTurn());

    }
    private System.Collections.IEnumerator ExecuteGuard()
    {
        isPlayerTurn = false;
        SetMenuState(BattleMenuState.Busy);

        // 防御用のフラグを立てる
        isGuading = true;
        DialogText.text = "みを まもっている！";
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(EnemyTurn());
    }

    private System.Collections.IEnumerator ExecuteUseItem(ItemData item)
    {
        isPlayerTurn = false;
        SetMenuState(BattleMenuState.Busy);
        DialogText.text = $"{item.ItemName} を つかった！";
        yield return new WaitForSeconds(0.8f);

        if (item.Type == ItemData.ItemType.HealHP)
        {
            float heal = item.Power;
            PlayerHP = Mathf.Min(PlayerMaxHP, PlayerHP + heal);
            DialogText.text = $"HPが {heal} かいふくした！";
        }
        else if (item.Type == ItemData.ItemType.HealMP)
        {
            // MP未実装ならメッセージのみ
            DialogText.text = "MPが　かいふくした！";
        }
        else
        {
            DialogText.text = "しかし　なにも　おこらなかった！";
        }

        UpdateUI();
        yield return new WaitForSeconds(0.8f);

        StartCoroutine(EnemyTurn());
    }



    private System.Collections.IEnumerator TryEscape()
    {
        // Random.valueは0~1の間の値をランダムに返してくれます
        bool success = Random.value < 0.5f;
        if (success)
        {
            DialogText.text = "うまく にげきれた！";
            Invoke(nameof(ReturnToField), 1.2f);
        }
        else
        {
            DialogText.text = "まわりこまれてしまった！";
            isPlayerTurn = false;
            SetMenuState(BattleMenuState.Busy);
            yield return new WaitForSeconds(0.8f);
            StartCoroutine(EnemyTurn());
        }
    }

    /// <summary>
    /// Buttonを生成
    /// </summary>
    void CreateButton(Transform root, string label,
        System.Action onClick)
    {
        if (MenuButtonPrefab == null || root == null)
        {
            return;
        }
        var btn = Instantiate(MenuButtonPrefab, root);
        btn.Setup(label, onClick);
    }

    void ClearChildren(Transform root)
    {
        if (root == null)
        {
            return;
        }
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// UnityEditorでAttackButtonのOnClickに設定
    /// </summary>
    public void OnAttackButton()
    {
        // プレイヤーのターンでなければ何もしない
        if (!isPlayerTurn)
        {
            return;
        }
        StartCoroutine(PlayerAttack());
    }

    private System.Collections.IEnumerator PlayerAttack()
    {
        isPlayerTurn = false;

        DialogText.text = "プレイヤーの攻撃！";

        // 1秒待つ
        yield return new WaitForSeconds(1f);
        // ダメージ計算で小数点切り上げ
        var damage =
            Mathf.Ceil(
                Random.Range(PlayerAttackMin, PlayerAttackMax)
                );
        EnemyHP -= damage;

        DialogText.text = $"{damage} のダメージ！";

        UpdateUI();

        yield return new WaitForSeconds(1f);

        // EnemyHP切れたら勝利
        if (EnemyHP <= 0f)
        {
            // 勝利
            Victory();
        }
        else
        { // 生きていれば敵行動へ
            StartCoroutine(EnemyTurn());
        }
    }

    private System.Collections.IEnumerator EnemyTurn()
    {
        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;

            DialogText.text = $"{enemy.Data.DisplayName} の攻撃！";

            if (enemy.Animator != null)
            {
                enemy.Animator.PlayAttack();
            }

            yield return new WaitForSeconds(1f);

            // 小数点切り上げで敵のダメージ計算仮
            var damage = Mathf.Ceil(
                    Random.Range(enemy.Data.AttackMin,
                    enemy.Data.AttackMax)
                    );

            // Playerが防御中
            if (isGuading)
            {
                damage = Mathf.Ceil(damage * 0.5f);
            }

            PlayerHP -= damage;

            DialogText.text = $"{damage} のダメージ！";

            UpdateUI();

            yield return new WaitForSeconds(1f);

            if (PlayerHP <= 0f)
            {
                // 敗北
                GameOver();
                yield break;
            }
        }

        // 全ての敵の行動が終わったら防御フラグを下ろす
        isGuading = false;

        isPlayerTurn = true;
        SetMenuState(BattleMenuState.Root);
        DialogText.text = "どうする？";
    }

    private bool CheckVictory()
    {
        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead) return false;
        }
        return true;
    }

    /// <summary>
    /// HPなどのUIの更新
    /// </summary>
    public void UpdateUI()
    {
        PlayerHPText.text = $"HP:{PlayerHP}/{PlayerMaxHP}";

        // 敵のステータスUIを更新
        if (EnemyStatusUIPrefab != null && EnemyStatusUIRoot != null)
        {
            // 数の不一致があれば再構築
            if (enemyStatusUIs.Count != enemies.Count)
            {
                ClearChildren(EnemyStatusUIRoot);
                enemyStatusUIs.Clear();
                foreach (var enemy in enemies)
                {
                    var ui = Instantiate(EnemyStatusUIPrefab, EnemyStatusUIRoot);
                    ui.Setup(enemy);
                    enemyStatusUIs.Add(ui);
                }
            }
            else
            {
                // 既存のUIを更新
                foreach (var ui in enemyStatusUIs)
                {
                    ui.Refresh();
                }
            }
        }
    }

    private IEnumerator Victory()
    {
        DialogText.text = "勝利！";

        int exp = 0;
        foreach (var enemy in enemies)
        {
            exp += enemy.Data.ExpReward;
        }

        int levelUps = 0;
        if (LevelSystem != null)
        {
            levelUps = LevelSystem.AddExp(exp);
        }

        ApplyPlayerStatus();

        UpdateUI();

        if (levelUps > 0)
        {
            DialogText.text +=
                $"\n{exp} EXP かくとく！" +
                $"\nレベルが {PlayerStatus.Level} になった！";
        }
        else
        {
            DialogText.text +=
                $"\n{exp} EXP かくとく！";
        }

        // 1秒待って
        yield return new WaitForSeconds(1f);

        int gold = 0;
        foreach (var enemy in enemies)
        {
            gold += enemy.Data.GoldReward;
        }
        // データ上のゴールドを増やす
        PlayerState.Instance.AddGold(gold);

        // ユーザーにGoldが増えたことを通知する
        DialogText.text = $"{gold} Gold かくとく！";


        // アニメーションは既に攻撃時の処理などで再生されているはずだが、
        // 念のため死んだ敵全員に対して行うならここで。
        bool isBossDefeated = false;
        foreach (var enemy in enemies)
        {
            if (enemy.Data.IsBoss)
            {
                isBossDefeated = true;
                break;
            }
        }

        if (isBossDefeated)
        {
            Invoke(nameof(ReturnToResult), 2f);
        }
        else
        {
            Invoke(nameof(ReturnToField), 2f);
        }
    }
    private void GameOver()
    {
        DialogText.text = "全滅です……";
        Invoke(nameof(ReturnToField), 2f);
    }
    private void ReturnToField()
    {
        if (PlayerState.Instance != null) PlayerState.Instance.CurrentHP = PlayerHP;
        SceneManager.LoadScene("Field_01");
    }

    private void ReturnToResult()
    {
        if (PlayerState.Instance != null) PlayerState.Instance.CurrentHP = PlayerHP;
        SceneManager.LoadScene("ResultScene");
    }
}
