# Tasks

> **작업 원칙**: C# 소스(`.cs`), `.asmdef`, `.inputactions`(JSON) 작성만 파일을 직접 한다. 패키지 설치, 컴파일, 코드 생성 설정, 프로젝트 설정 확인, 테스트, Play 모드 확인은 `unity-cli` / `unity-pipeline`으로 실행 중인 Editor를 통해 하고, `.meta`·`.unity`·`ProjectSettings/*.asset`은 손으로 편집하지 않는다. 생성된 `PlayerControls.cs`도 편집하지 않는다.
>
> **컴파일 확인**: `unity command recompile` 후 `recompile_status`가 `completed`/`up_to_date`가 될 때까지 확인한다.
>
> **테스트 실행**: EditMode는 `unity command run_tests -- --mode editor`. PlayMode는 `run_tests -- --mode playmode --async_tests true` 후 `test_status`를 호출마다 시간 제한을 두고 폴링한다.
>
> **대화상자**: Editor가 대화상자(재시작 등)를 띄우면 pipeline 명령이 응답하지 않는다. 오준석님께 처리를 요청하고 기다린다.

## 0. 작업 브랜치와 기준 상태

- [x] 0.1 `test/add-player-tests`에서 `git switch -c refactor/adopt-input-system`으로 작업 브랜치를 만들고 `git branch --show-current`로 확인한다. `Packages/manifest.json`의 미커밋 `com.unity.pipeline` 변경을 이번 작업 커밋에 포함할지 오준석님께 확인한다(design.md D6)
- [x] 0.2 `unity status`가 `ready`인지 확인하고 `set_autotick` 실행 후 컴파일 확인을 통과시킨다. EditMode(29개)·PlayMode(5개) 테스트가 모두 통과하는 기준 상태를 기록한다

## 1. 패키지 설치와 입력 백엔드 전환

- [x] 1.1 Editor의 패키지 명령(`unity command`의 packages 태그)으로 `com.unity.inputsystem@1.20.1`을 설치한다. Editor가 띄우는 백엔드 활성화·재시작 대화상자는 오준석님께 처리를 요청한다. 재시작 후 `unity status`가 `ready`가 되면 `set_autotick`을 다시 실행한다
- [x] 1.2 `eval`로 Active Input Handling(`activeInputHandler`)이 1(Input System 전용)인지 확인한다. 아니면 오준석님께 Player Settings에서 Input System Package (New)로 바꾸고 재시작하도록 요청한 뒤 다시 확인한다
- [x] 1.3 이 시점에는 `UnityPlayerInput`이 아직 레거시 Input을 쓰므로, `editor_play`로 Play 모드에서 레거시 Input 예외가 나는지 콘솔에서 확인하고 기록한다(전환이 실제로 적용됐다는 증거). `editor_stop`. `git status`로 설치가 `Assets/`에 자동 생성한 파일이 있는지 확인하고, 있으면 오준석님께 처리 방법을 확인한다
- [x] 1.4 `manifest.json`·`packages-lock.json`·`ProjectSettings/ProjectSettings.asset`에서 이번 변경분만 스테이징해(design.md D6) 커밋한다

## 2. Input Actions 에셋

- [x] 2.1 `Assets/scripts/Input/PlayerControls.inputactions`를 design.md D1의 표대로 작성한다(`Move`의 1D Axis 컴포지트에 `whichSideWins=Negative`)
- [x] 2.2 `eval`로 이 에셋 임포터의 "Generate C# Class"를 켜고(클래스 이름 `PlayerControls`, 같은 폴더) `SaveAndReimport`한다. `PlayerControls.cs`가 생성되고 컴파일 확인을 통과하는지, `eval`로 `typeof(PlayerControls).Assembly`가 `MetalSlug`인지 확인한다
- [x] 2.3 `MetalSlug.asmdef`가 `Unity.InputSystem`을 참조하도록 하고(2.2 컴파일에 필요하면 2.2 전에 한다) 컴파일 확인을 통과시킨 뒤 2단계를 커밋한다

## 3. UnityPlayerInput 교체와 키 매핑 테스트

- [x] 3.1 `UnityPlayerInput`을 design.md D2대로 `PlayerControls` 기반으로 다시 구현하고(`IDisposable`), `Player.OnDestroy`와 `SetInput`에서 기존 입력을 해제한다. 컴파일 확인을 통과시키고 `Assets/scripts`에 `UnityEngine.Input`·`KeyCode` 사용이 남지 않았는지 grep으로 확인한다
- [x] 3.2 `manifest.json`에 `"testables": ["com.unity.inputsystem"]`를 추가하고, EditMode 테스트 asmdef에 `Unity.InputSystem`, `Unity.InputSystem.TestFramework` 참조를 추가한다. 컴파일 확인을 통과시킨다
- [x] 3.3 `Assets/Tests/EditMode/UnityPlayerInputTests.cs`(`InputTestFixture` 상속)에 design.md D4의 케이스를 작성하고 EditMode 테스트가 전부 통과하는지 확인한다. `.inputactions`의 `whichSideWins`를 잠시 빼서 동시 입력 테스트가 실패하는지 확인한 뒤 되돌린다
- [x] 3.4 EditMode·PlayMode 테스트가 모두 통과하는지 확인하고, `editor_play`에서 콘솔 에러가 0건인지 확인한 뒤 3단계를 커밋한다

## 4. 실제 키 경로 확인

- [x] 4.1 Play 모드에서 가짜 입력 없이 `simulate_key`로 확인한다. `RightArrow` down → 약 1초 → up 동안 Player가 오른쪽으로 이동하며 Move였다가 Idle로 돌아오는지, `UpArrow` press로 Jump 상태가 되고 착지하는지, `Space` press로 `AudioSource`가 재생되는지를 `eval`로 읽은 상태·위치·소리로 확인한다. 콘솔 에러 0건을 확인하고 `editor_stop`한다
- [x] 4.2 오준석님이 Play 모드에서 직접 키보드로 체크리스트를 확인한다. ① ←/→ 이동·좌우 반전 ② 키를 떼면 idle ③ ↑ 점프·공중 좌우 이동 ④ 착지 후 idle/run 복귀 ⑤ Space 공격 애니메이션·gunfire 소리 ⑥ ←·→를 함께 누르면 왼쪽으로 이동

## 5. 통합 확인

- [x] 5.1 EditMode·PlayMode 테스트 전체 통과를 확인한다. `git status`로 새 파일마다 `.meta`가 있는지, `Main.unity`·`Player.controller`가 변경되지 않았는지, 커밋에 `com.unity.pipeline` 변경이 0.1의 결정대로만 들어갔는지 확인한다

## Workflow follow-up

- 모든 작업이 끝나면 머지·푸시·PR 생성을 하지 않고, 변경 요약(`git diff test/add-player-tests...refactor/adopt-input-system --stat`)을 보고한 뒤 오준석님의 리뷰를 기다린다.
- 리뷰 후 반영과 아카이브(`/opsx:archive`)는 오준석님의 요청이 있을 때 진행한다.
