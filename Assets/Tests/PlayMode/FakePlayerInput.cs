using UnityEngine;

/// <summary>
/// PlayMode 테스트용 입력입니다. Horizontal은 설정한 값을 유지하고,
/// PressJump/PressAttack은 호출 후 처음 읽힌 프레임 한 프레임 동안만 true입니다(GetKeyDown과 같은 의미).
/// </summary>
public class FakePlayerInput : IPlayerInput
{
    private bool _jumpRequested;
    private bool _attackRequested;
    private int _jumpFrame = -1;
    private int _attackFrame = -1;

    public float Horizontal { get; set; }
    public bool JumpPressed => ReadPress(ref _jumpRequested, ref _jumpFrame);
    public bool AttackPressed => ReadPress(ref _attackRequested, ref _attackFrame);

    public void PressJump() => _jumpRequested = true;
    public void PressAttack() => _attackRequested = true;

    private static bool ReadPress(ref bool requested, ref int pressedFrame)
    {
        if (requested)
        {
            requested = false;
            pressedFrame = Time.frameCount;
        }

        return pressedFrame == Time.frameCount;
    }
}
