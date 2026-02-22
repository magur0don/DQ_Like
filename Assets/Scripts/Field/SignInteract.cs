using UnityEngine;

public class SignInteract : MonoBehaviour, IInteractable
{
    [TextArea]
    public string Message = "ここは　はじまりの　むら　です";

    public void Interact()
    {
        // 既存の会話が進められるなら進める
        if (DialogUI.Instance != null &&
            DialogUI.Instance.TryNextIfOpen())
        {
            return;
        }

        if (DialogUI.Instance != null)
        {
            DialogUI.Instance.ShowSimpleMessage(Message);
        }
        else
        {
            Debug.Log($"[Sign] {Message}");
        }
    }
}
