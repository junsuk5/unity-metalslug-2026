/// <summary>
/// 플레이어 상태가 사용하는 기능입니다. 상태는 이 인터페이스만 보고 "무엇을 할지"를 결정합니다.
/// </summary>
public interface IPlayerContext
{
    IPlayerInput Input { get; }
    bool IsGrounded { get; }
    void Move(float direction);
    void Jump();
    void SetWalking(bool isWalking);

    PlayerState IdleState { get; }
    PlayerState MoveState { get; }
    PlayerState JumpState { get; }
    void ChangeState(PlayerState next);
}
