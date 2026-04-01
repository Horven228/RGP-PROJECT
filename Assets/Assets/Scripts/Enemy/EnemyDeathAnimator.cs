using UnityEngine;

public class EnemyDeathAnimator : MonoBehaviour, IDeathAnimator
{
    [SerializeField] private Animator _enemyAnimator;
    [SerializeField] private string _deathBoolName = "Death";

    private void Start()
    {
        if (_enemyAnimator == null)
            _enemyAnimator = GetComponent<Animator>();
    }

    public void PlayDeathAnimation()
    {
        if (_enemyAnimator != null && !string.IsNullOrEmpty(_deathBoolName))
        {
            _enemyAnimator.SetBool(_deathBoolName, true);
        }
    }
}