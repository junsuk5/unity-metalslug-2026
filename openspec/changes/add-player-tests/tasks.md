# Tasks

> **작업 원칙**: C# 소스(`.cs`)와 `.asmdef`(JSON) 작성만 파일을 직접 한다. 컴파일, `.meta` 생성, 테스트 실행, 씬·인스펙터 값 확인은 `unity-cli` / `unity-pipeline` 스킬로 실행 중인 Unity Editor를 통해 처리하고, `.unity`·`.controller`·`.meta` 파일은 손으로 편집하지 않는다.
>
> **컴파일 확인**: `unity command recompile` 후 `unity command recompile_status`가 `completed`/`up_to_date`가 될 때까지 확인하고, `failed=true`면 `errors`를 고친 뒤 다시 실행한다.
>
> **테스트 실행**: EditMode는 `unity command run_tests -- --mode editor`. PlayMode는 `unity command run_tests -- --mode playmode --async_tests true`로 시작한 뒤 `unity command test_status`가 `completed`가 될 때까지 폴링한다(design.md D8). 실패하면 기대값을 넓히기 전에 원인을 보고한다.

## 0. 작업 브랜치와 에디터 연결 준비

- [x] 0.1 `refactor/player-srp`에서 `git switch -c test/add-player-tests`로 작업 브랜치를 만들고 `git branch --show-current`로 확인한다. 이 변경의 `openspec/changes/add-player-tests/` 파일이 함께 넘어왔는지 확인한다
- [x] 0.2 `unity status`로 Editor가 `ready`인지, `unity command`에 `recompile`·`run_tests`·`eval`이 있는지 확인하고, `set_autotick` 실행 후 컴파일 확인으로 기준 상태(에러 없음)를 확인한다

## 1. 런타임 어셈블리

- [x] 1.1 `Assets/scripts/MetalSlug.asmdef`(이름 `MetalSlug`, `autoReferenced: true`)를 추가하고 컴파일 확인을 통과시킨다. `eval`로 `typeof(Player).Assembly.GetName().Name`이 `MetalSlug`인지 확인한다
- [x] 1.2 `eval`로 씬의 Player·FollowCam 컴포넌트가 null이 아니고 Player `_gunfireClip`이 `gunfire` 클립인지 확인한다. `git status`에서 `Main.unity`가 변경되지 않았는지 확인한 뒤 `editor_play` → 프레임 진행 → 콘솔 에러 0건을 확인하고 `editor_stop`한다

## 2. 테스트 어셈블리

- [x] 2.1 `Assets/Tests/EditMode/MetalSlug.Tests.EditMode.asmdef`와 `Assets/Tests/PlayMode/MetalSlug.Tests.PlayMode.asmdef`를 design.md D2대로 추가한다(EditMode만 Editor 전용, `MetalSlug`·TestRunner 참조, `nunit.framework.dll`, `UNITY_INCLUDE_TESTS`). 컴파일 확인을 통과시키고 `run_tests`가 두 모드 모두 에러 없이 실행되는지 확인한다(테스트 0개)
- [x] 2.2 `Assets/scripts/AssemblyInfo.cs`에 `[assembly: InternalsVisibleTo("MetalSlug.Tests.PlayMode")]`를 추가하고 컴파일 확인을 통과시킨다

## 3. PlayerMotor 테스트

- [x] 3.1 `PlayerMotor.Move(direction, deltaTime)`로 바꾸고 `Player.Move`가 `Time.deltaTime`을 넘기도록 한다(design.md D5). 컴파일 확인을 통과시킨다
- [x] 3.2 `Assets/Tests/EditMode/PlayerMotorTests.cs`에 D7의 PlayerMotor 케이스(좌우 방향 전환과 크기 유지, `speed × deltaTime` 이동량, `Jump`의 속도 초기화)를 작성하고 EditMode `run_tests`에서 모두 통과하는지 확인한다. 테스트에서 만든 GameObject는 `TearDown`에서 `Object.DestroyImmediate`로 정리한다

## 4. GroundDetector 테스트

- [x] 4.1 `Assets/Tests/EditMode/GroundDetectorTests.cs`에 D7의 GroundDetector 케이스(`Ground` 태그 접촉·이탈 시 IsGrounded와 이벤트 값, 다른 태그 무시)를 작성하고 EditMode `run_tests`에서 모두 통과하는지 확인한다

## 5. 상태 전이 테스트

- [x] 5.1 `Assets/Tests/EditMode/Fakes/FakePlayerInput.cs`와 `FakePlayerContext.cs`를 design.md D6대로 추가한다(호출 기록, 실제 상태 객체 3개 노출). 컴파일 확인을 통과시킨다
- [x] 5.2 `PlayerIdleStateTests.cs`, `PlayerMoveStateTests.cs`, `PlayerJumpStateTests.cs`에 D7의 상태 케이스를 작성하고 EditMode `run_tests`에서 모두 통과하는지 확인한다. 테스트 하나의 `ChangeState` 대상을 일부러 틀리게 바꿔 실패하는지 한 번 확인한 뒤 되돌린다

## 6. Player 통합 테스트

- [x] 6.1 `Player`에 `internal void SetInput(IPlayerInput)`과 `internal IState CurrentState`를 추가한다(design.md D4). 컴파일 확인을 통과시키고 게임 코드에서 이 멤버를 쓰는 곳이 없는지 grep으로 확인한다
- [x] 6.2 `Assets/Tests/PlayMode/FakePlayerInput.cs`(프레임 단위 눌림, D6)와 `PlayerPlayModeTests.cs`의 공통 준비를 작성한다: `EditorSceneManager.LoadSceneAsyncInPlayMode`로 `Assets/Scenes/Main.unity`를 불러오고, Player를 찾아 `SetInput`으로 가짜 입력을 주입한 뒤 착지할 때까지(최대 3초) 기다린다. 이동 테스트(오른쪽 1초 → 6 ± 0.35 이동·Move, 놓으면 Idle, 왼쪽 → 좌우 반전)를 작성하고 PlayMode `run_tests`에서 통과하는지 확인한다
- [x] 6.3 점프 테스트(제자리 점프 상승 높이 2.45 ± 0.15, 착지 후 Idle·`Grounded` true, 공중 오른쪽 유지 시 착지 후 Move)와 공격 테스트(`AudioSource` 재생, 애니메이터 `attack` 상태를 거쳐 복귀)를 추가하고 PlayMode `run_tests`에서 통과하는지 확인한다
- [ ] 6.4 PlayMode `run_tests`를 3번 연속 실행해 매번 통과하는지(타이밍 때문에 가끔 실패하지 않는지) 확인한다

## 7. 통합 확인

- [ ] 7.1 EditMode·PlayMode `run_tests`를 모두 실행해 전부 통과하는지 확인한다. `git status`로 새 `.cs`·`.asmdef`마다 `.meta`가 있는지, `Main.unity`·`Player.controller`·`ProjectSettings/EditorBuildSettings.asset`이 변경되지 않았는지 확인한다

## Workflow follow-up

- 모든 작업이 끝나면 머지·푸시·PR 생성을 하지 않고, `test/add-player-tests` 브랜치의 변경 요약(`git diff refactor/player-srp...test/add-player-tests --stat`)을 보고한 뒤 오준석님의 리뷰를 기다린다.
- 리뷰 후 반영과 아카이브(`/opsx:archive`)는 오준석님의 요청이 있을 때 진행한다.
