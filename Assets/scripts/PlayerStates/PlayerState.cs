using FSM;

/// <summary>
/// 플레이어 상태들의 공통 베이스 클래스입니다.
/// </summary>
public abstract class PlayerState : IState
{
    protected readonly IPlayerContext context;

    protected PlayerState(IPlayerContext context)
    {
        this.context = context;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}
