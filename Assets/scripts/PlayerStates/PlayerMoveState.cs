/// <summary>
/// 좌우로 걷는 상태입니다.
/// </summary>
public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(IPlayerContext context) : base(context) { }

    public override void Enter()
    {
        context.SetWalking(true);
    }

    public override void Update()
    {
        if (context.Input.JumpPressed)
        {
            context.ChangeState(context.JumpState);
            return;
        }

        if (context.Input.Horizontal == 0f)
        {
            context.ChangeState(context.IdleState);
            return;
        }

        context.Move(context.Input.Horizontal);
    }
}
