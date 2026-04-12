using UnityEngine;

public class PlayerDeathAnimator : MonoBehaviour, IDeathAnimator
{
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private string _deathBoolName = "Death";

    private void Start()
    {
        if (_playerAnimator == null)
            _playerAnimator = GetComponent<Animator>();
    }

    public void PlayDeathAnimation()
    {
        if (_playerAnimator != null && !string.IsNullOrEmpty(_deathBoolName))
        {
            _playerAnimator.SetBool(_deathBoolName, true);
        }
    }
}