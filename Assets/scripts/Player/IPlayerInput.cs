/// <summary>
/// 플레이어 입력을 프레임마다 제공합니다.
/// </summary>
public interface IPlayerInput
{
    /// <summary>좌(-1), 우(1), 입력 없음(0)</summary>
    float Horizontal { get; }

    /// <summary>이번 프레임에 점프 키를 눌렀는지</summary>
    bool JumpPressed { get; }

    /// <summary>이번 프레임에 공격 키를 눌렀는지</summary>
    bool AttackPressed { get; }
}
