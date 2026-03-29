using UnityEngine;

public class PlayBGMOnStart : MonoBehaviour
{
    [Header("このシーンで鳴らすBGM")]
    public AudioClip BGMClip;

    private void Start()
    {
        if (BGMManager.Instance != null && BGMClip != null)
        {
            BGMManager.Instance.PlayBGM(BGMClip);
        }
    }
}
