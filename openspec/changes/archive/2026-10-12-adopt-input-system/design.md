# Design

## Context

- 입력은 `IPlayerInput`(`Horizontal`, `JumpPressed`, `AttackPressed`)으로 분리되어 있다. 구현체 `UnityPlayerInput`만 `UnityEngine.Input.GetKey/GetKeyDown`을 쓴다. `Player.Awake`가 `new UnityPlayerInput()`을 만들고, 테스트는 `internal SetInput`으로 바꿔 끼운다.
- Active Input Handling은 Input Manager(0)이고, `com.unity.inputsystem`은 설치되어 있지 않다. 이 Editor(6000.3.23f1)의 권장 버전은 1.20.1이다.
- 게임 스크립트는 `MetalSlug` asmdef(`Assets/scripts/`)에 있고, 테스트는 EditMode(Editor 전용)·PlayMode asmdef로 나뉘어 있다. 현재 EditMode 29개, PlayMode 5개 테스트가 있으며 PlayMode 테스트는 가짜 입력을 쓴다.
- `Packages/manifest.json`에는 이 작업과 별개로 커밋되지 않은 `com.unity.pipeline` 추가가 남아 있다.
- `com.unity.pipeline`의 `simulate_key`(down/up/press)는 Input System이 있을 때만 동작하고, 키 이벤트를 큐에 넣는다.

동기는 proposal.md의 Why 참고.

## Goals / Non-Goals

**Goals:**
- 같은 키·같은 우선순위로 동작하는 Input System 기반 `UnityPlayerInput`
- 키 매핑을 Input Actions 에셋에서 관리
- 실제 키 경로의 자동 검증(EditMode `InputTestFixture` + Play 모드 `simulate_key`)

**Non-Goals:**
- 새 키·장치 추가, 키 재설정 UI
- `IPlayerInput` 인터페이스나 상태 코드 변경

## Decisions

### D1. Input Actions 에셋 + 생성된 C# 클래스를 쓰고, `PlayerInput` 컴포넌트는 쓰지 않는다

`Assets/scripts/Input/PlayerControls.inputactions`에 액션 맵 `Player`를 정의하고, 임포터의 "Generate C# Class"로 같은 폴더에 `PlayerControls.cs`를 생성한다. `MetalSlug` asmdef 폴더 안에 두어야 `UnityPlayerInput`과 같은 어셈블리로 컴파일된다.

| 액션 | 타입 | 바인딩 |
|---|---|---|
| `Move` | Value / Axis | 1D Axis 컴포지트: negative `<Keyboard>/leftArrow`, positive `<Keyboard>/rightArrow`, `whichSideWins=Negative` |
| `Jump` | Button | `<Keyboard>/upArrow` |
| `Attack` | Button | `<Keyboard>/space` |

`whichSideWins=Negative`로 두 키를 함께 눌렀을 때 ←가 이기게 해서 기존 우선순위를 지킨다. 기본값 `Neither`는 0을 돌려주므로 동작이 바뀐다.

- 대안: `PlayerInput` 컴포넌트 → 씬에 컴포넌트를 추가해야 하고, 입력이 `Player` 밖에서 오는 구조가 된다.
- 대안: 프로젝트 전역 액션(`InputSystem.actions`) → 액션을 문자열로 찾아야 해서 타입 안정성이 없다.
- 대안: `Keyboard.current` 직접 사용 → 오준석님 결정으로 제외(키 매핑을 에셋으로 관리하려는 목적과 맞지 않음).

`.inputactions`는 JSON 소스 파일이라 `.cs`·`.asmdef`처럼 직접 작성한다. "Generate C# Class" 설정은 `.meta`에 저장되므로 `.meta`를 손으로 고치지 않는다. 대신 `eval`로 임포터의 직렬화 필드(`m_GenerateWrapperCode` 등)를 바꾸고 `SaveAndReimport`해서 Editor가 반영하게 한다.

### D2. `UnityPlayerInput`이 `PlayerControls`를 소유하고 `IDisposable`을 구현한다

```csharp
public class UnityPlayerInput : IPlayerInput, IDisposable
{
    private readonly PlayerControls _controls = new PlayerControls();
    public UnityPlayerInput() => _controls.Player.Enable();
    public float Horizontal => _controls.Player.Move.ReadValue<float>();
    public bool JumpPressed => _controls.Player.Jump.WasPressedThisFrame();
    public bool AttackPressed => _controls.Player.Attack.WasPressedThisFrame();
    public void Dispose()
    {
        if (Application.isPlaying) { _controls.Dispose(); return; }
        _controls.Disable();
        Object.DestroyImmediate(_controls.asset);
    }
}
```

생성된 `PlayerControls.Dispose()`는 `Object.Destroy`를 호출하는데, EditMode에서는 이 호출이 에러를 낸다. 그래서 Play 중이 아니면(EditMode 테스트) 액션을 끄고 `DestroyImmediate`로 에셋을 해제한다(구현 중 확인, 오준석님 결정).

`WasPressedThisFrame`은 같은 프레임 안에서 여러 번 읽어도 같은 값이라 `GetKeyDown`과 의미가 같다. 디지털 키에서 `Move`는 -1/0/1만 돌려준다. 클래스 이름은 그대로 둔다. "Unity가 제공하는 입력을 읽는 구현"이라는 뜻이 여전히 맞고, 이름을 바꾸면 파일·`.meta`를 옮겨야 한다.

`Player.OnDestroy`에서 `(Input as IDisposable)?.Dispose()`를 호출한다. 테스트 전용 `SetInput`도 기존 입력을 해제한 뒤 바꾼다. 해제하지 않으면 활성화된 액션이 Player보다 오래 남는다.

- 대안: `Player.OnEnable/OnDisable`에서 Enable/Disable → `IPlayerInput`에 수명 메서드를 추가해야 한다. 가짜 입력까지 영향을 받는다.

### D3. Active Input Handling은 Input System 전용(1)으로 둔다

레거시 Input을 쓰는 곳이 `UnityPlayerInput` 하나뿐이라 함께 바꾸면 된다. 레거시 API가 다시 쓰이면 바로 예외로 드러난다.

- **설정 방법:** 패키지 설치 시 Editor가 백엔드 활성화·재시작 대화상자를 띄운다. 이 대화상자는 Editor의 메인 스레드를 막아 pipeline 명령이 응답하지 않으므로 오준석님이 직접 처리한다.
- **확인:** 재시작 후 `eval`로 값이 1인지 확인하고, 아니면 Player Settings에서 1로 바꾸고 다시 재시작한다.

- 대안: Both(2) → 오준석님 결정으로 제외.

### D4. 키 매핑은 `InputTestFixture`로 EditMode에서 테스트한다

`manifest.json`에 `"testables": ["com.unity.inputsystem"]`를 추가하고, EditMode 테스트 asmdef가 `Unity.InputSystem`, `Unity.InputSystem.TestFramework`를 참조한다. `UnityPlayerInputTests : InputTestFixture`는 가상 키보드를 추가하고 `Press`/`Release`로 다음 케이스를 확인한다.
- 입력 없음 → 0
- ← → -1, → → 1
- ←·→ 동시 → -1
- ↑를 누른 업데이트에서만 `JumpPressed`
- Space를 누른 업데이트에서만 `AttackPressed`

`InputTestFixture`는 테스트마다 Input System 상태를 격리하고 원래대로 되돌리므로 실행 중인 Editor의 입력에 영향을 주지 않는다.

- 대안: PlayMode에서만 확인 → 키 매핑 같은 순수 로직을 매번 씬을 띄워 확인하게 된다.

### D5. 실제 키 경로는 `simulate_key`로 한 번 끝에서 끝까지 확인하고, 마지막에 오준석님이 직접 확인한다

Play 모드에서 가짜 입력을 주입하지 않은 상태로 `simulate_key` down/up을 보낸다. (구현 중 확인: `simulate_key`는 이벤트를 넣은 직후 `InputSystem.Update()`를 수동으로 호출해서, 한 번 누름은 `Player.Update` 전에 소비되어 `WasPressedThisFrame`으로 관찰되지 않는다. 그래서 점프·공격은 이벤트를 큐에만 넣고 프레임 업데이트가 처리하게 하는 `run_script`로 확인했다. 이동은 `simulate_key`로 확인했다.) →를 누른 채 이동, ↑ 점프, Space 공격이 실제 `Player`에 전달되는지 상태·위치·소리로 확인한다. 이 확인은 자동 테스트로 만들지 않는다. Play 모드의 실제 키보드 장치에 의존해서, `InputTestFixture`(D4)와 가짜 입력 PlayMode 테스트가 이미 다루는 내용을 중복하기 때문이다.

### D6. 매니페스트의 무관한 변경은 섞지 않는다

`manifest.json`·`packages-lock.json`에는 미리 들어 있던 `com.unity.pipeline` 추가가 있다. 커밋할 때 이번 변경분(`com.unity.inputsystem`, `testables`)만 스테이징한다(`git diff`에서 해당 부분만 뽑아 `git apply --cached`). pipeline 변경을 함께 커밋할지는 작업 시작 때 오준석님께 확인한다.

## Risks / Trade-offs

- [패키지 설치·설정 변경 대화상자가 Editor를 막아 자동화가 멈춤] → 설치 직후 오준석님께 대화상자 처리를 요청한다. 재시작 후 `unity status --until-ready`(시간 제한을 둠)로 다시 연결하고 `set_autotick`을 다시 실행한다.
- [Input System 설치가 `Assets/`에 기본 에셋(예: 프로젝트 전역 액션)을 자동으로 만들 수 있음] → 설치 후 `git status`로 새 파일을 확인한다. 쓰지 않는 자동 생성 에셋은 지우기 전에 오준석님께 확인한다.
- [`whichSideWins` 설정 누락으로 ←·→ 동시 입력 동작이 바뀜] → D4의 동시 입력 테스트로 잡는다.
- [Input System 업데이트 모드에 따라 `WasPressedThisFrame`이 `Update`에서 놓칠 수 있음] → 기본값(Dynamic Update)을 유지하고 D4·D5에서 확인한다. Input System 설정 에셋은 만들지 않는다.
- [생성된 `PlayerControls.cs`를 손으로 고치면 재생성 때 덮어써짐] → 생성 파일은 편집하지 않고 커밋만 한다. 매핑 변경은 `.inputactions`에서 한다.
- [Active Input Handling 변경은 되돌릴 때도 재시작이 필요함] → 롤백 절차를 Migration Plan에 적는다.

## Migration Plan

`test/add-player-tests`에서 작업 브랜치 `refactor/adopt-input-system`을 만들어 진행한다. 순서는 다음과 같고, 단계마다 컴파일·테스트 후 커밋한다.

1. 패키지 설치와 설정 전환
2. Input Actions 에셋·코드 생성
3. `UnityPlayerInput` 교체와 키 매핑 테스트
4. 실제 키 확인

컴파일, 코드 생성, 설정 확인, 테스트, Play 모드 확인은 `unity-cli`/`unity-pipeline`으로 실행 중인 Editor를 통해 하고, `.meta`·`.unity`·`ProjectSettings` 파일은 손으로 편집하지 않는다.

**롤백:** 코드 커밋을 되돌리고 Active Input Handling을 Input Manager(0)로 되돌린 뒤 Editor를 재시작하고 패키지를 제거한다. 끝나면 머지하지 않고 리뷰를 기다린다.
