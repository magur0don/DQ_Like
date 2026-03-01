using UnityEngine;

[System.Serializable]
public class EnemyBattleInfo
{
    public EnemyData Data;
    public float CurrentHP;
    public GameObject ModelInstance;
    public EnemyAnimator Animator;

    public bool IsDead => CurrentHP <= 0;

    public EnemyBattleInfo(EnemyData data)
    {
        Data = data;
        CurrentHP = data.MaxHP;
    }
}
