# Design

## Context

현재 구조(커밋 `8c5602f` 기준):

- `Player : MonoBehaviour`가 `Update`에서 `Input.GetKey`로 입력을 읽고, `StateMachine.Update()`를 호출한 뒤, 같은 프레임에 Space 키로 공격을 처리한다.
- 상태(`PlayerIdleState`, `PlayerMoveState`, `PlayerJumpState`)는 `PlayerState` 베이스를 통해 `Player`를 직접 참조하고, `player.StateMachine.ChangeState(player.XxxState)`로 전이한다.
- 착지 판정은 `OnCollisionEnter2D/Exit2D`에서 `Ground` 태그로 `IsGrounded`와 애니메이터 `Grounded`를 함께 갱신한다.
- 씬(`Main.unity`)의 Player 컴포넌트에 `_gunfireClip`이 직렬화되어 있다(필드 이름에 묶인 참조).
- 프로젝트에 asmdef가 없어 모든 스크립트가 `Assembly-CSharp`에 들어간다.

동기는 proposal.md의 Why 참고.

## Goals / Non-Goals

**Goals:**
- 클래스 하나가 하나의 변경 이유만 갖도록 나눈다.
- 상태 클래스가 Unity API나 `Player` 구체 타입 없이 `IPlayerContext`만 보고 동작하게 한다.
- 씬·프리팹·애니메이터 수정 없이 리팩토링을 끝낸다.

**Non-Goals:**
- 게임 플레이 동작 변경(낙하 상태 추가, 공중 점프 제한, 물리 기반 이동 전환 등).
- 새 Input System 도입.
- asmdef 도입과 자동화 테스트 작성.
- 여러 `Ground` 콜라이더에 동시에 닿았을 때의 기존 착지 판정 동작 수정.

## Decisions

### D1. 분리한 부품은 일반 C# 클래스로 만들고 `Player`가 생성한다

`PlayerMotor`, `GroundDetector`, `PlayerAnimator`, `PlayerAudio`, `PlayerWeapon`, `UnityPlayerInput` 모두 생성자로 필요한 Unity 컴포넌트(`Transform`, `Rigidbody2D`, `Animator`, `AudioSource`)를 받는 일반 클래스다. `Player.Awake`에서 조립한다.

- 대안: 각각을 MonoBehaviour 컴포넌트로 만들어 인스펙터에서 연결 → 씬 수정이 필요하고, 컴포넌트 간 실행 순서 문제가 생긴다. 지금 규모에서는 이득보다 비용이 크다.
- 결과: 씬 변경이 없고, 나중에 테스트 시 가짜 객체로 바꾸기 쉽다.

### D2. 충돌 콜백은 `Player`가 받아서 `GroundDetector`에 넘긴다

Unity 충돌 콜백은 MonoBehaviour에서만 받을 수 있다. `Player.OnCollisionEnter2D/Exit2D`는 `_groundDetector.OnContactEnter(collider)` / `OnContactExit(collider)`만 호출하고, `Ground` 태그 판정은 `GroundDetector` 안에서 한다. `GroundDetector`는 `IsGrounded`와 `event Action<bool> GroundedChanged`를 제공하고, `Player`가 이 이벤트를 `PlayerAnimator.SetGrounded`에 연결한다.

- 대안: `GroundDetector`를 MonoBehaviour로 → D1과 같은 이유로 제외.
- 대안: Raycast/OverlapBox 기반 판정으로 교체 → 동작이 바뀌므로 Non-Goal.

### D3. 상태는 `IPlayerContext`에만 의존한다

```csharp
public interface IPlayerContext
{
    IPlayerInput Input { get; }
    bool IsGrounded { get; }
    void Move(float direction);
    void Jump();
    void SetWalking(bool isWalking);

    PlayerState IdleState { get; }
    PlayerState MoveState { get; }
    PlayerState JumpState { get; }
    void ChangeState(PlayerState next);
}
```

`Player`가 이 인터페이스를 구현하고 각 메서드는 해당 부품에 위임만 한다. 상태 전이는 `ChangeState` 하나로 모아 `StateMachine`을 상태에 노출하지 않는다.

- 대안: 상태에 `PlayerMotor`, `PlayerAnimator` 등을 각각 주입 → 생성자 인자가 늘고, 상태가 "어떤 부품이 무엇을 하는지"까지 알게 된다. 상태는 "무엇을 할지"만 알게 하는 편이 낫다.
- 대안: 상태 ID enum으로 전이(`ChangeState(PlayerStateId.Idle)`) → 지금은 상태가 3개뿐이라 간접 단계만 늘어난다.

### D4. 입력은 `IPlayerInput`이 프레임마다 값을 제공한다

`float Horizontal`(-1/0/1), `bool JumpPressed`, `bool AttackPressed`. `UnityPlayerInput`은 기존과 같은 키(←/→, ↑, Space)와 같은 우선순위(← 우선)로 `UnityEngine.Input`을 직접 읽는 프로퍼티로 구현한다. `GetKeyDown` 계열은 같은 프레임 안에서 여러 번 읽어도 같은 값이므로 별도 캐싱(`ReadInput()`)은 없앤다.

### D5. 애니메이터 파라미터는 `PlayerAnimator`에서 해시로 관리한다

`Animator.StringToHash("Walk")` 등을 `static readonly int`로 두고 `SetWalking`, `SetGrounded`, `PlayAttack`만 공개한다. 파라미터 이름 문자열은 이 클래스에만 남는다.

### D6. 공격은 `PlayerWeapon.Fire()`가 담당하고, 상태가 아니라 `Player.Update`에서 호출한다

공격은 모든 상태에서 가능하므로 기존처럼 `StateMachine.Update()` 다음에 `if (_input.AttackPressed) _weapon.Fire();`로 처리한다. `PlayerWeapon`은 `PlayerAnimator.PlayAttack()`과 `PlayerAudio.PlayGunfire()`를 호출한다. 이후 총알 발사가 추가될 자리다.

### D7. 직렬화 필드와 수치

- `_gunfireClip`은 이름 그대로 `Player`에 남겨 씬 참조를 유지하고 `PlayerAudio` 생성자에 넘긴다.
- `MoveSpeed`(6), `JumpForce`(500)는 `Player`의 `[SerializeField]` 필드로 옮기고 기본값을 같게 둔다. 씬에 값이 직렬화되지 않았으므로 기본값이 그대로 적용된다.
- `AudioSource`가 없으면 추가하는 기존 로직은 `Player.Awake`에 남긴다(부품 조립 책임).

### D8. 파일 배치

- 새 클래스: `Assets/scripts/Player/` (`IPlayerInput.cs`, `UnityPlayerInput.cs`, `IPlayerContext.cs`, `PlayerMotor.cs`, `GroundDetector.cs`, `PlayerAnimator.cs`, `PlayerAudio.cs`, `PlayerWeapon.cs`)
- `Player.cs`와 `PlayerStates/`는 현재 위치에 둔다. 옮기면 `.meta` GUID 관리가 필요하고 씬 참조가 깨질 위험이 있기 때문이다.

## Risks / Trade-offs

- [`.meta` 누락으로 GUID가 달라지거나 씬의 `Player` 스크립트 참조가 깨짐] → `Player.cs`는 이름·위치·클래스명을 바꾸지 않는다. 새 파일의 `.meta`는 `unity command recompile`로 Editor가 생성하게 두고 함께 커밋한다. `.meta`·씬·에셋 파일은 직접 편집하지 않는다.
- [`_gunfireClip` 이름 변경 시 사운드 연결 끊김] → D7대로 필드 이름을 유지한다.
- [실행 순서 변화로 미묘한 동작 차이(예: 같은 프레임의 점프와 공격)] → `Update` 안의 호출 순서(상태 갱신 → 공격)를 그대로 유지하고, 각 단계 후 플레이로 확인한다.
- [자동화 테스트가 없어 회귀를 수동 확인에 의존] → tasks.md에 수동 확인 체크리스트를 둔다. 테스트는 asmdef 도입 후 별도 변경으로 진행한다.
- [클래스 수 증가로 작은 프로젝트에서 과설계로 느껴질 수 있음] → 인터페이스는 입력과 컨텍스트 두 개만 두고, 나머지는 인터페이스 없이 구체 클래스로 시작한다.

## Migration Plan

C# 소스 편집 외의 Unity 작업(컴파일, `.meta` 생성, 씬 값 확인, Play 모드)은 `unity-cli`/`unity-pipeline` 스킬로 실행 중인 Editor를 통해 수행한다. 키보드 입력이 필요한 플레이 확인은 Editor를 띄운 상태에서 사용자가 직접 조작한다.

`refactoring` 브랜치에서 만든 작업 브랜치 `refactor/player-srp`에서 진행한다. 단계별로 나눠 각 단계마다 컴파일·플레이 확인 후 커밋한다. 문제가 생기면 해당 커밋만 되돌린다. 작업이 끝나면 머지하지 않고 리뷰를 기다린다. 씬·에셋을 건드리지 않으므로 별도 데이터 마이그레이션은 없다.
