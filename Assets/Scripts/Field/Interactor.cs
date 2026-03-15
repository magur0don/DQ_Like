using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerのInteraction（相互作用）を行うためのクラス
/// </summary>
public class Interactor : MonoBehaviour
{
    /// <summary>
    /// インタラクト可能な距離
    /// </summary>
    public float Distance = 2.0f;

    public LayerMask InteractLayer;

    public float EyeHeight = 0.5f;

    // PlayerInputのInteract(Eキー)が押された時に呼ばれます
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        // ダイアログが開いている場合は、次へ進める（または閉じる）
        if (GameState.IsDialogOpen && DialogUI.Instance != null)
        {
            DialogUI.Instance.Next();
            return;
        }

        TryInteract();
    }

    private void TryInteract()
    {
        Vector3 origin = transform.position + Vector3.up * EyeHeight;
        // レイを発射する
        Ray ray = new Ray(origin, transform.forward);

        // デバッグ用の線
        Debug.DrawRay(origin, transform.forward * Distance, Color.yellow, 0.5f);

        // レイが何かに当たったか判定
        if (Physics.Raycast(ray, out RaycastHit hit, Distance, InteractLayer))
        {
            // 当たったオブジェクトからIInteractableを取得
            var interactable = hit.collider.GetComponent<IInteractable>();
            // インタラクト実行
            interactable?.Interact();
        }
    }

    // スペースキーなど、他の「次へ」操作用（もしあれば）
    public void OnMessageNext(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (GameState.IsDialogOpen && DialogUI.Instance != null)
        {
            DialogUI.Instance.Next();
        }
    }
}
