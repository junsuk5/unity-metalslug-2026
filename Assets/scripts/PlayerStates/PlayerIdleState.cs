/// <summary>
/// 제자리에 서 있는 상태입니다.
/// </summary>
public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(IPlayerContext context) : base(context) { }

    public override void Enter()
    {
        context.SetWalking(false);
    }

    public override void Update()
    {
        if (context.Input.JumpPressed)
        {
            context.ChangeState(context.JumpState);
        }
        else if (context.Input.Horizontal != 0f)
        {
            context.ChangeState(context.MoveState);
        }
    }
}
