using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    /// <summary>
    /// RetryButtonが押されたときの処理
    /// </summary>
    public void OnRetryButtonClicked()
    {
        // Field_01.sceneに遷移する
        SceneManager.LoadScene("Field_01");
    }

    /// <summary>
    /// ExitButtonが押されたときの処理
    /// </summary>
    public void OnExitButtonClicked()
    {
        // アプリケーションを終了する
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
