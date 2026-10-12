/// <summary>
/// 테스트가 값을 직접 정하는 입력입니다.
/// </summary>
public class FakePlayerInput : IPlayerInput
{
    public float Horizontal { get; set; }
    public bool JumpPressed { get; set; }
    public bool AttackPressed { get; set; }
}
