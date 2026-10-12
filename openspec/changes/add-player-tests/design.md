# Design

## Context

`refactor-player-srp` 이후 구조:

- `Player : MonoBehaviour, IPlayerContext`가 `PlayerAnimator`, `PlayerMotor`, `GroundDetector`, `PlayerWeapon`(+`PlayerAudio`)을 Awake에서 조립하고 `StateMachine`을 private으로 가진다. 입력은 `public IPlayerInput Input { get; private set; }`이고 Awake에서 `UnityPlayerInput`으로 생성된다.
- 상태(`PlayerIdleState` 등)는 `IPlayerContext`만 의존하는 일반 C# 클래스다.
- `PlayerMotor.Move(direction)`가 안에서 `Time.deltaTime`을 읽는다.
- asmdef가 없어 모든 스크립트가 `Assembly-CSharp`에 있다. `Assets/` 아래 `.cs`는 `Assets/scripts/`에만 있다(`Assets/Plugins/Editor`는 비어 있음).
- `com.unity.test-framework` 1.6.0, `com.unity.ext.nunit` 2.0.5가 이미 설치되어 있다.
- `Main.unity`는 Build Settings에 없다. 씬에는 Player 1개(프리팹 아님, Rigidbody2D gravityScale 2, 콜라이더 1개), `Ground` 태그 오브젝트 1개가 있고 Enemy는 없다.
- `refactor-player-srp` 검증 때 측정한 기준값: 이동 속도 6.0/s, 제자리 점프 상승 높이 2.45.

동기는 proposal.md의 Why 참고.

## Goals / Non-Goals

**Goals:**
- 상태 전이 규칙을 Unity 없이 밀리초 단위로 검증하는 EditMode 테스트
- 실제 씬 구성(콜라이더·중력·애니메이터·오디오 클립)으로 Player 조립을 검증하는 소수의 PlayMode 테스트
- 리플렉션 없이 테스트할 수 있는 정식 주입 지점

**Non-Goals:**
- 커버리지 목표 설정, CI 연동
- 테스트를 위해 인터페이스를 늘리는 것(`IPlayerMotor` 등)
- 실제 키보드 입력(`UnityPlayerInput`) 자동 테스트. 레거시 Input은 시뮬레이션할 수 없으므로 새 Input System 전환 변경에서 다룬다.

## Decisions

### D1. 런타임 asmdef 하나로 `Assets/scripts/` 전체를 묶는다

`Assets/scripts/MetalSlug.asmdef`(이름 `MetalSlug`, `autoReferenced: true`)를 둔다. 테스트 어셈블리는 `Assembly-CSharp`를 참조할 수 없으므로 게임 코드를 이름 있는 어셈블리로 옮겨야 한다.

- 대안: `Assets/scripts/Player/`에만 asmdef → `Player.cs`와 `PlayerStates/`가 `Assembly-CSharp`에 남아 테스트할 수 없고, 옮기면 다시 나눠야 한다.
- 대안: 기능별로 여러 asmdef(FSM, Player, Enemy…) → 지금 규모에서는 참조 관리 비용만 늘어난다.

### D2. 테스트 어셈블리는 EditMode·PlayMode 둘로 나누고, 둘 다 Editor 전용으로 둔다

- `Assets/Tests/EditMode/MetalSlug.Tests.EditMode.asmdef`
- `Assets/Tests/PlayMode/MetalSlug.Tests.PlayMode.asmdef`

둘 다 `MetalSlug`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`를 참조하고 `nunit.framework.dll`을 precompiled reference로, `UNITY_INCLUDE_TESTS`를 define constraint로 둔다. PlayMode 어셈블리도 `includePlatforms: ["Editor"]`로 둔다. D3에서 `EditorSceneManager`를 쓰기 때문이다.

- 대안: PlayMode 어셈블리를 모든 플랫폼용으로 → 플레이어 빌드에서 테스트를 돌릴 수 있지만, 그러려면 `Main`을 Build Settings에 넣어야 한다(D3). 지금은 에디터에서만 돌리면 충분하다.

### D3. PlayMode 테스트는 실제 `Main` 씬을 `EditorSceneManager.LoadSceneAsyncInPlayMode`로 불러온다

Player 조립 버그(이벤트 구독 누락, 수치 전달 오류, 컴포넌트 누락)는 실제 씬 구성에서만 드러난다. 경로(`Assets/Scenes/Main.unity`)로 불러오므로 Build Settings를 바꾸지 않아도 된다.

- 대안: `SceneManager.LoadScene("Main")` → `Main`을 Build Settings에 추가해야 해서 프로젝트 설정이 바뀐다.
- 대안: 테스트 코드에서 GameObject·콜라이더·땅을 직접 구성 → 씬과 구성이 달라져 정작 확인하려는 "씬의 Player가 제대로 조립되는가"를 검증하지 못한다.

### D4. `Player`에 `internal` 테스트 주입 지점을 두고 `InternalsVisibleTo`로 공개한다

```csharp
internal void SetInput(IPlayerInput input) => Input = input;
internal IState CurrentState => _stateMachine.CurrentState;
```

`Assets/scripts/AssemblyInfo.cs`에 `[assembly: InternalsVisibleTo("MetalSlug.Tests.PlayMode")]`를 둔다. EditMode 테스트는 public인 상태·부품만 쓰므로 internal 접근이 필요 없다.

- 대안: `public` → 게임 코드 어디서나 입력을 바꿀 수 있게 된다.
- 대안: 리플렉션 → backing field 이름에 묶여 깨지기 쉽다. 이번 변경을 하는 이유 그 자체다.
- 대안: `[SerializeField]`로 입력 공급자를 받기 → 씬 수정이 필요하다.

### D5. `PlayerMotor.Move(direction, deltaTime)`로 시간을 인자로 받는다

EditMode에서는 `Time.deltaTime`이 일정하지 않아 이동량을 확정적으로 검증할 수 없다. `IPlayerContext.Move(direction)`는 그대로 두고, `Player.Move`가 `_motor.Move(direction, Time.deltaTime)`을 호출한다. 상태 코드와 동작은 바뀌지 않는다.

- 대안: `ITime` 같은 시간 추상화 주입 → 호출하는 곳이 하나뿐이라 인자 하나로 충분하다.

### D6. 가짜 객체는 직접 짜고, 어셈블리마다 따로 둔다

- EditMode `FakePlayerInput`: 세 프로퍼티를 테스트가 직접 설정한다. `FakePlayerContext`: `Input`·`IsGrounded`를 설정할 수 있고, `Move`/`Jump`/`SetWalking`/`ChangeState` 호출을 기록한다. 상태 객체 3개는 실제 상태 클래스로 만들어 `IdleState` 등으로 노출한다.
- PlayMode `FakePlayerInput`: `Horizontal`은 설정한 값을 유지한다. `PressJump()`/`PressAttack()`은 호출 후 처음 읽힌 프레임 한 프레임 동안만 `true`를 돌려준다(`GetKeyDown`과 같은 의미).
- 대안: NSubstitute → DLL 반입과 asmdef 참조가 늘어난다. 인터페이스 두 개에는 이득이 작다.
- 대안: 공용 테스트 유틸 asmdef → 두 가짜 입력은 의미가 달라(즉시 값 vs 프레임 단위 눌림) 공유할 이득이 없다.

### D7. 테스트 범위

**EditMode**
- Idle: 진입 시 `SetWalking(false)`, 점프 입력 → Jump, 좌우 입력 → Move, 점프와 좌우를 함께 누르면 Jump가 우선, 입력 없으면 전이 없음
- Move: 진입 시 `SetWalking(true)`, 점프 → Jump, 입력 없음 → Idle, 입력 유지 → 그 방향으로 `Move`
- Jump: 진입 시 `Jump()` 호출, 아직 땅을 떠나지 않았으면(IsGrounded=true) 전이 없음, 땅을 떠난 뒤 착지하면 입력에 따라 Move/Idle, 공중 좌우 입력 → `Move`와 `SetWalking(true)`
- `PlayerMotor`: 오른쪽/왼쪽 이동 시 `localScale.x` 부호(크기 유지), `speed × deltaTime`만큼 이동, `Jump`가 기존 속도를 0으로 초기화
- `GroundDetector`: `Ground` 태그와 닿으면 IsGrounded=true와 이벤트(true), 떨어지면 false와 이벤트(false), 다른 태그는 무시

**PlayMode** (시작 시 Player가 착지할 때까지 대기)
- 오른쪽 1초 → x 증가량이 6 ± 0.35, `localScale.x > 0`, 상태 Move. 놓으면 Idle
- 왼쪽 → `localScale.x < 0`
- 제자리 점프 → 최고 상승 높이 2.45 ± 0.15, 착지 후 Idle, 애니메이터 `Grounded` true
- 점프 후 공중에서 오른쪽 유지 → 착지 후 Move
- 공격 → `AudioSource`가 재생되고 애니메이터가 `attack` 상태를 거쳐 돌아온다

대기는 고정 시간보다 "조건 충족 또는 제한 시간 초과"로 쓴다(예: 착지할 때까지 최대 3초). 수치 허용 오차는 `refactor-player-srp` 검증 때 관찰한 프레임 타이밍 오차(속도 ±0.25, 점프 직후 한 프레임의 높이 차)를 근거로 정했다.

### D8. 테스트 실행은 실행 중인 Editor의 `run_tests`로 한다

`unity command run_tests -- --mode EditMode` / `-- --mode PlayMode`. `unity test`는 별도 배치 Editor를 띄우므로 같은 프로젝트를 연 Editor와 충돌한다.

## Risks / Trade-offs

- [스크립트가 `MetalSlug` 어셈블리로 옮겨지면서 씬의 스크립트 참조가 끊김] → Unity는 MonoBehaviour를 스크립트 `.meta` GUID로 참조하므로 어셈블리가 바뀌어도 유지된다. asmdef 추가 후 `eval`로 씬의 Player·FollowCam 컴포넌트 스크립트가 null이 아닌지, `_gunfireClip`이 유지되는지 확인하고, `Main.unity`가 git에서 변경되지 않았는지 확인한다.
- [PlayMode 테스트가 프레임 타이밍에 따라 가끔 실패함] → 조건 대기와 허용 오차를 쓴다(D7). 실패하면 기대값부터 넓히지 말고 원인을 보고한다.
- [PlayMode 테스트가 Editor의 열린 씬에 영향] → Test Framework가 PlayMode 테스트 후 원래 씬 구성을 복원한다. 테스트 전후로 `Main.unity`가 dirty·변경되지 않았는지 확인한다.
- [`internal` 멤버가 프로덕션 코드에 테스트용 표면을 추가] → 두 멤버로 최소화하고 테스트 어셈블리에만 공개한다.
- [씬 의존 PlayMode 테스트는 씬이 바뀌면 함께 고쳐야 함] → PlayMode 테스트는 5개 안팎으로 유지하고, 경우의 수는 EditMode에서 다룬다.

## Migration Plan

`refactor/player-srp` 브랜치에서 작업 브랜치 `test/add-player-tests`를 만들어 진행한다(`refactor-player-srp`가 아직 머지되지 않았으므로). asmdef 도입 → 프로덕션 코드 주입 지점 → EditMode 테스트 → PlayMode 테스트 순으로 단계마다 컴파일·테스트 후 커밋한다. 컴파일, `.meta` 생성, 테스트 실행, 씬 값 확인은 `unity-cli`/`unity-pipeline`으로 실행 중인 Editor를 통해 하고, `.unity`·`.meta` 파일은 손으로 편집하지 않는다. 문제가 생기면 해당 커밋만 되돌린다. asmdef 커밋을 되돌리면 스크립트는 `Assembly-CSharp`로 돌아간다. 끝나면 머지하지 않고 리뷰를 기다린다.
