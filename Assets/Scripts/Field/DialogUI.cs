using UnityEngine;
using TMPro;
using System.Collections;

public class DialogUI : MonoBehaviour
{
    /// <summary>
    /// どこでもDialogUIにアクセスできるように
    /// staticで宣言
    /// </summary>
    public static DialogUI Instance;

    public GameObject Panel;

    public TextMeshProUGUI NameText;

    public TextMeshProUGUI MessageText;

    [Header("続きがあるときのNextHint (Optional)")]
    public GameObject NextHint;

    /// <summary>
    /// はい、いいえボタンの親
    /// </summary>
    public GameObject YesNoButtonBG;


    /// <summary>
    /// 1文字表示する時間
    /// </summary>
    private float charInterval = 0.2f;

    /// <summary>
    /// メッセージの現在の行
    /// </summary>
    private string[] currentLines;

    /// <summary>
    /// 行のインデックス
    /// </summary>
    private int lineIndex;

    private Coroutine typingCoroutine;

    /// <summary>
    /// タイプ中判定
    /// </summary>
    private bool isTyping = false;


    private void Awake()
    {
        Instance = this;
        // ゲーム開始時はダイアログは表示しない
        Panel.SetActive(false);
        if (NextHint != null)
        {
            NextHint.SetActive(false);
        }
    }

    /// <summary>
    /// 通常ダイアログの設定と表示
    /// </summary>
    public void Show(DialogData dialogData)
    {
        // DialogDataのデータに入力がない場合
        if (dialogData == null ||
            dialogData.MessgeLines == null ||
            dialogData.MessgeLines.Length == 0)
        {
            Debug.LogWarning("DialogData無効です");
            return;
        }

        // boolの値を直接GameObjectのActiveの値に変更
        YesNoButtonBG.SetActive(dialogData.ShowYesNo);

        GameState.IsDialogOpen = true;

        NameText.text = dialogData.Speaker;

        currentLines = dialogData.MessgeLines;
        lineIndex = 0;

        Panel.SetActive(true);
        // 最初の行を表示
        ShowLine(lineIndex);
    }

    /// <summary>
    /// 鍵やアイテム取得時のダイアログ表示
    /// </summary>
    public void ShowItemDialog(string dialogMessage)
    {
        // dialogMessage空
        if (dialogMessage == string.Empty)
        {
            Debug.LogWarning("dialogMessage空です");
            return;
        }

        // YesNoボタンは表示しない
        YesNoButtonBG.SetActive(false);

        GameState.IsDialogOpen = true;

        NameText.text = string.Empty;

        string[] itemLines = new string[1];
        itemLines[0] = $"{dialogMessage}　を手に入れた" ;
        currentLines = itemLines;
        lineIndex = 0;

        Panel.SetActive(true);
        // 最初の行を表示
        ShowLine(lineIndex);
    }

    /// <summary>
    /// 看板などの単一メッセージダイアログ
    /// </summary>
    public void ShowSimpleMessage(string dialogMessage)
    {
        // dialogMessage空
        if (dialogMessage == string.Empty)
        {
            Debug.LogWarning("dialogMessage空です");
            return;
        }

        // YesNoボタンは表示しない
        YesNoButtonBG.SetActive(false);

        GameState.IsDialogOpen = true;

        NameText.text = string.Empty;

        string[] itemLines = new string[1];
        itemLines[0] = $"{dialogMessage}";
        currentLines = itemLines;
        lineIndex = 0;

        Panel.SetActive(true);
        // 最初の行を表示
        ShowLine(lineIndex);
    }




    /// <summary>
    /// UIをCloseして、状態を戻す
    /// </summary>
    public void Close()
    {
        // まず止める
        StopTypingIfNeeded();
        GameState.IsDialogOpen = false;
        Panel.SetActive(false);
        if (NextHint != null)
        {
            NextHint.SetActive(false);
        }
        currentLines = null;
        lineIndex = 0;
    }

    /// <summary>
    /// 次の会話への遷移
    /// </summary>
    public void Next()
    {
        // パネル開いてない
        if (!Panel.activeSelf)
        {
            return;
        }
        // タイプ中なら
        if (isTyping)
        {
            // 即表示
            FinishCurrentLineInstant();
            return;
        }
        // 次の行へ
        lineIndex++; // インクリメント(n+1すること)

        if (currentLines != null &&
            lineIndex < currentLines.Length)
        {
            ShowLine(lineIndex);
        }
        else
        {
            Close();
        }
    }

    /// <summary>
    /// index行目の文字を表示
    /// </summary>
    /// <param name="index"></param>
    private void ShowLine(int index)
    {
        // まず止める
        StopTypingIfNeeded();
        // 表示する前に空にする
        MessageText.text = string.Empty;
        // 次がある表示のオブジェクト・非表示
        if (NextHint != null)
        {
            NextHint.SetActive(true);
        }

        typingCoroutine =
            StartCoroutine(TypeLine(currentLines[index]));
    }
    /// <summary>
    /// 文字送りコルーチン
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        // １文字ずつ出して待つ
        foreach (char c in line)
        {
            MessageText.text += c;
            yield return new WaitForSeconds(charInterval);
        }
        isTyping = false;

        if (NextHint != null)
        {
            NextHint.SetActive(true);
        }
        // 自動で次の行へ移行よう
        // Next();
        typingCoroutine = null;
    }

    private void FinishCurrentLineInstant()
    {
        if (currentLines == null)
        {
            return;
        }
        // まず止める
        StopTypingIfNeeded();
        // 現在の行を全表示
        MessageText.text = currentLines[lineIndex];
        // タイプ中終了
        isTyping = false;
        if (NextHint != null)
        {
            NextHint.SetActive(true);
        }
    }

    /// <summary>
    /// 必要ならコルーチンを止める
    /// </summary>
    private void StopTypingIfNeeded()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        isTyping = false;
    }

    /// <summary>
    /// ダイアログUIが開いていれば次の行を表示
    /// </summary>
    /// <returns></returns>
    public bool TryNextIfOpen()
    {
        // ダイアログ開いてなければ反応しない
        if (!Panel.activeSelf)
        {
            return false;
        }
        // 通常の Next と同じ挙動
        Next();
        return true;
    }

    public void OnYes()
    {
        Close();
    }

    public void OnNo()
    {
        Close();
    }
}
