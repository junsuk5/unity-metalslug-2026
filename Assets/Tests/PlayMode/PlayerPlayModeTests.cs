using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// 실제 Main 씬의 Player에 가짜 입력을 주입해 조립이 맞는지 끝에서 끝까지 확인합니다.
/// </summary>
public class PlayerPlayModeTests
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const float MoveSpeed = 6f;
    private const float SpeedTolerance = 0.35f;
    private const float JumpRise = 2.45f;
    private const float JumpRiseTolerance = 0.15f;

    private Player _player;
    private FakePlayerInput _input;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
#if UNITY_EDITOR
        // Main은 Build Settings에 없으므로 에디터에서만 경로로 불러옵니다.
        yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
            ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
        Assert.Ignore("Main 씬이 Build Settings에 없어 플레이어 빌드에서는 실행하지 않습니다");
#endif

        _player = UnityEngine.Object.FindFirstObjectByType<Player>();
        Assert.IsNotNull(_player, "Main 씬에 Player가 없습니다");
        _input = new FakePlayerInput();
        _player.SetInput(_input);

        yield return WaitUntil(() => _player.IsGrounded && _player.CurrentState is PlayerIdleState, 3f, "시작 후 착지");
    }

    [UnityTest]
    public IEnumerator 오른쪽을_누르면_초당_6만큼_이동하고_놓으면_Idle이_된다()
    {
        float x0 = _player.transform.position.x;
        float t0 = Time.time;

        _input.Horizontal = 1f;
        yield return new WaitForSeconds(1f);

        float speed = (_player.transform.position.x - x0) / (Time.time - t0);
        Assert.AreEqual(MoveSpeed, speed, SpeedTolerance);
        Assert.Greater(_player.transform.localScale.x, 0f);
        Assert.IsInstanceOf<PlayerMoveState>(_player.CurrentState);

        _input.Horizontal = 0f;
        yield return null;
        yield return null;

        Assert.IsInstanceOf<PlayerIdleState>(_player.CurrentState);
        float stoppedX = _player.transform.position.x;
        yield return new WaitForSeconds(0.2f);
        Assert.AreEqual(stoppedX, _player.transform.position.x, 1e-4f);
    }

    [UnityTest]
    public IEnumerator 왼쪽을_누르면_왼쪽을_보고_왼쪽으로_이동한다()
    {
        float x0 = _player.transform.position.x;

        _input.Horizontal = -1f;
        yield return new WaitForSeconds(0.5f);

        Assert.Less(_player.transform.localScale.x, 0f);
        Assert.Less(_player.transform.position.x, x0);
        Assert.IsInstanceOf<PlayerMoveState>(_player.CurrentState);
    }

    [UnityTest]
    public IEnumerator 제자리_점프하면_정해진_높이까지_올랐다가_착지해_Idle이_된다()
    {
        float y0 = _player.transform.position.y;
        float maxY = y0;

        _input.PressJump();
        yield return WaitUntil(() => !_player.IsGrounded, 1f, "점프 후 땅을 떠남");
        Assert.IsInstanceOf<PlayerJumpState>(_player.CurrentState);
        Assert.IsFalse(_player.GetComponent<Animator>().GetBool("Grounded"));

        float deadline = Time.time + 3f;
        while (!_player.IsGrounded)
        {
            Assert.Less(Time.time, deadline, "3초 안에 착지하지 못했습니다");
            maxY = Mathf.Max(maxY, _player.transform.position.y);
            yield return null;
        }

        yield return null;

        Assert.AreEqual(JumpRise, maxY - y0, JumpRiseTolerance);
        Assert.IsInstanceOf<PlayerIdleState>(_player.CurrentState);
        Assert.IsTrue(_player.GetComponent<Animator>().GetBool("Grounded"));
    }

    [UnityTest]
    public IEnumerator 공중에서_오른쪽을_누른_채_착지하면_Move가_된다()
    {
        float x0 = _player.transform.position.x;

        _input.PressJump();
        yield return WaitUntil(() => !_player.IsGrounded, 1f, "점프 후 땅을 떠남");
        _input.Horizontal = 1f;
        yield return WaitUntil(() => _player.IsGrounded, 3f, "착지");
        yield return null;

        Assert.IsInstanceOf<PlayerMoveState>(_player.CurrentState);
        Assert.Greater(_player.transform.position.x, x0);
    }

    [UnityTest]
    public IEnumerator 공격하면_총소리가_나고_공격_애니메이션을_거쳐_돌아온다()
    {
        var audioSource = _player.GetComponent<AudioSource>();
        var animator = _player.GetComponent<Animator>();
        Assert.IsNotNull(audioSource, "Player가 AudioSource를 준비하지 않았습니다");

        _input.PressAttack();
        yield return WaitUntil(() => audioSource.isPlaying, 0.5f, "총소리 재생");
        yield return WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("attack"), 1f, "공격 애니메이션 진입");
        yield return WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("idle"), 2f, "idle 애니메이션 복귀");

        Assert.IsInstanceOf<PlayerIdleState>(_player.CurrentState);
    }

    /// <summary>조건이 참이 될 때까지 기다리고, 제한 시간을 넘기면 실패합니다.</summary>
    private static IEnumerator WaitUntil(Func<bool> condition, float timeout, string description)
    {
        float deadline = Time.time + timeout;
        while (!condition())
        {
            if (Time.time > deadline)
            {
                Assert.Fail($"{description}: {timeout}초 안에 조건을 만족하지 못했습니다");
            }

            yield return null;
        }
    }
}
