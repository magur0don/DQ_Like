using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    private AudioSource audioSource;

    private void Awake()
    {
        // シングルトン化：シーンを遷移しても破棄されないようにする
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // AudioSourceコンポーネントがなければ追加する
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            // BGMはループ再生にする
            audioSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// BGMを再生します。
    /// （既に同じBGMが流れている場合は、リセットされずそのまま流れ続けます）
    /// </summary>
    /// <param name="clip">再生するAudioClip</param>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // すでに同じ曲が再生中の場合は何もしない（途切れさせないため）
        if (audioSource.isPlaying && audioSource.clip == clip)
        {
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    /// <summary>
    /// BGMを停止します。
    /// </summary>
    public void StopBGM()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
