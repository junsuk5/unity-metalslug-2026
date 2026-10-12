using UnityEngine;

/// <summary>
/// 키보드(←/→ 이동, ↑ 점프, Space 공격)로 입력을 읽습니다. ←와 →를 함께 누르면 ←가 우선합니다.
/// </summary>
public class UnityPlayerInput : IPlayerInput
{
    public float Horizontal
    {
        get
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                return -1f;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                return 1f;
            }

            return 0f;
        }
    }

    public bool JumpPressed => Input.GetKeyDown(KeyCode.UpArrow);

    public bool AttackPressed => Input.GetKeyDown(KeyCode.Space);
}
