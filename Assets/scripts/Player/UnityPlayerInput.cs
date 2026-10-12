using System;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Input System의 PlayerControls 액션(←/→ 이동, ↑ 점프, Space 공격)으로 입력을 읽습니다.
/// ←와 →를 함께 누르면 ←가 우선합니다. 다 쓰면 Dispose로 액션을 해제해야 합니다.
/// </summary>
public class UnityPlayerInput : IPlayerInput, IDisposable
{
    private readonly PlayerControls _controls = new PlayerControls();

    public UnityPlayerInput()
    {
        _controls.Player.Enable();
    }

    public float Horizontal => _controls.Player.Move.ReadValue<float>();

    public bool JumpPressed => _controls.Player.Jump.WasPressedThisFrame();

    public bool AttackPressed => _controls.Player.Attack.WasPressedThisFrame();

    public void Dispose()
    {
        if (Application.isPlaying)
        {
            _controls.Dispose();
            return;
        }

        // EditMode(테스트)에서는 Destroy를 쓸 수 없습니다.
        _controls.Disable();
        Object.DestroyImmediate(_controls.asset);
    }
}
