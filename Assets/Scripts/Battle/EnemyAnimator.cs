using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void PlayDie()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
    }
}
