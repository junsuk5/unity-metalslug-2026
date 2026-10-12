using System.Collections.Generic;

/// <summary>
/// 상태가 요청한 동작을 기록하는 가짜 컨텍스트입니다. 상태 객체 3개는 실제 상태 클래스입니다.
/// </summary>
public class FakePlayerContext : IPlayerContext
{
    public FakePlayerContext()
    {
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        JumpState = new PlayerJumpState(this);
    }

    public FakePlayerInput FakeInput { get; } = new FakePlayerInput();
    public IPlayerInput Input => FakeInput;
    public bool IsGrounded { get; set; } = true;

    public PlayerState IdleState { get; }
    public PlayerState MoveState { get; }
    public PlayerState JumpState { get; }

    /// <summary>Move에 넘어온 방향을 호출 순서대로 기록합니다.</summary>
    public List<float> Moves { get; } = new List<float>();
    public int JumpCount { get; private set; }
    /// <summary>마지막으로 설정한 걷기 애니메이션 값. 호출하지 않았으면 null입니다.</summary>
    public bool? Walking { get; private set; }
    /// <summary>마지막으로 요청한 전이 대상. 요청하지 않았으면 null입니다.</summary>
    public PlayerState ChangedTo { get; private set; }

    public void Move(float direction) => Moves.Add(direction);
    public void Jump() => JumpCount++;
    public void SetWalking(bool isWalking) => Walking = isWalking;
    public void ChangeState(PlayerState next) => ChangedTo = next;
}
