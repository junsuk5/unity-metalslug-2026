/// <summary>
/// 플레이어의 공격(애니메이션 + 사운드)을 처리합니다.
/// </summary>
public class PlayerWeapon
{
    private readonly PlayerAnimator _animator;
    private readonly PlayerAudio _audio;

    public PlayerWeapon(PlayerAnimator animator, PlayerAudio audio)
    {
        _animator = animator;
        _audio = audio;
    }

    public void Fire()
    {
        _animator.PlayAttack();
        _audio.PlayGunfire();
    }
}
