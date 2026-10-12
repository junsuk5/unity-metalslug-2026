using System;
using UnityEngine;

/// <summary>
/// "Ground" 태그 콜라이더와의 접촉으로 착지 여부를 판정합니다.
/// </summary>
public class GroundDetector
{
    private const string GroundTag = "Ground";

    public bool IsGrounded { get; private set; }

    /// <summary>땅에 닿거나 떨어질 때마다 호출됩니다.</summary>
    public event Action<bool> GroundedChanged;

    public void OnContactEnter(Collider2D collider)
    {
        if (collider.CompareTag(GroundTag))
        {
            SetGrounded(true);
        }
    }

    public void OnContactExit(Collider2D collider)
    {
        if (collider.CompareTag(GroundTag))
        {
            SetGrounded(false);
        }
    }

    private void SetGrounded(bool isGrounded)
    {
        IsGrounded = isGrounded;
        GroundedChanged?.Invoke(isGrounded);
    }
}
