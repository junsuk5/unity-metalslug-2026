using UnityEngine;

/// <summary>
/// 플레이어 애니메이터 파라미터를 설정합니다. 파라미터 이름은 이 클래스에서만 관리합니다.
/// </summary>
public class PlayerAnimator
{
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int AttackHash = Animator.StringToHash("attack");

    private readonly Animator _animator;

    public PlayerAnimator(Animator animator)
    {
        _animator = animator;
    }

    public void SetWalking(bool isWalking)
    {
        _animator.SetBool(WalkHash, isWalking);
    }

    public void SetGrounded(bool isGrounded)
    {
        _animator.SetBool(GroundedHash, isGrounded);
    }

    public void PlayAttack()
    {
        _animator.SetTrigger(AttackHash);
    }
}
