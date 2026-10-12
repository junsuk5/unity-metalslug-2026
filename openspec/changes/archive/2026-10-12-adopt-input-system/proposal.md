# Proposal

## Why

플레이어 입력은 레거시 `UnityEngine.Input`(Input Manager)으로 읽고 있다. 이 방식에는 두 가지 한계가 있다.

- 키 배치가 코드에 박혀 있다.
- 키 입력을 주입할 방법이 없어서 실제 키 경로를 자동으로 테스트할 수 없다. `refactor-player-srp`와 `add-player-tests`에서 실제 키 확인은 매번 수동 조작에 의존했다.

`IPlayerInput`으로 입력을 이미 분리해 두었으므로, 구현 하나만 바꾸면 새 Input System으로 옮길 수 있다.

## What Changes

- `com.unity.inputsystem` 패키지(이 Editor 권장 버전 1.20.1)를 설치하고, Active Input Handling을 **Input System 전용**으로 바꾼다(레거시 Input 비활성화, Editor 재시작 필요).
- Input Actions 에셋 `PlayerControls`(액션 맵 `Player`: `Move`, `Jump`, `Attack`)을 추가하고, 생성된 C# 클래스로 읽는다.
- `UnityPlayerInput`이 `UnityEngine.Input` 대신 `PlayerControls`를 읽도록 다시 구현한다. 키 배치는 그대로 둔다(←/→ 이동, ↑ 점프, Space 공격, ←·→를 함께 누르면 ← 우선).
- `Player`가 입력 객체의 수명을 정리하도록 `OnDestroy`에서 입력을 해제한다.
- Input System 테스트 지원(`InputTestFixture`)으로 `UnityPlayerInput`의 키 매핑 EditMode 테스트를 추가한다. `simulate_key`로 실제 키 경로를 끝에서 끝까지 확인한다.
- 게임 플레이 동작은 바뀌지 않는다.

## Capabilities

### New Capabilities
<!-- 없음: 입력 백엔드만 바꾸고 동작은 바꾸지 않으므로 skip_specs: true -->

### Modified Capabilities
<!-- 없음 -->

## Impact

- 패키지: `Packages/manifest.json`에 `com.unity.inputsystem` 의존성과 `testables: ["com.unity.inputsystem"]` 추가
- 프로젝트 설정: `ProjectSettings/ProjectSettings.asset`의 Active Input Handling이 Input Manager(0)에서 Input System(1)으로 바뀐다
- 추가: `Assets/scripts/Input/PlayerControls.inputactions`, 생성된 `PlayerControls.cs`, `Assets/Tests/EditMode/UnityPlayerInputTests.cs`
- 수정: `Assets/scripts/Player/UnityPlayerInput.cs`, `Assets/scripts/Player.cs`, `Assets/scripts/MetalSlug.asmdef`(`Unity.InputSystem` 참조), EditMode 테스트 asmdef(Input System·테스트 프레임워크 참조)
- 영향 없음 확인: 레거시 Input을 쓰는 코드는 `UnityPlayerInput` 하나뿐이다. `Main` 씬에 EventSystem이 없고, `com.unity.pipeline`도 레거시 Input을 쓰지 않는다.
- 선행 조건: `test/add-player-tests` 브랜치(리뷰 대기 중) 위에서 작업한다.
- 범위 밖: 게임패드·WASD 등 키 추가, 키 재설정(rebinding) UI, `PlayerInput` 컴포넌트 사용(씬 수정이 필요함)
