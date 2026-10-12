# Tasks

> **작업 원칙**: C# 소스(`.cs`) 편집만 파일을 직접 수정한다. 컴파일, `.meta` 생성, 씬·에셋·인스펙터 확인, Play 모드 실행은 모두 `unity-cli` / `unity-pipeline` 스킬로 실행 중인 Unity Editor를 통해 처리하고, `.unity`·`.controller`·`.meta` 파일은 손으로 편집하지 않는다.
>
> **컴파일 확인(이하 "컴파일 확인")**: `unity command recompile` 후 `unity command recompile_status`를 `completed`/`up_to_date`가 될 때까지 확인하고, `failed=true`면 `errors`를 고친 뒤 다시 실행한다.
>
> **플레이 확인(이하 "체크리스트")**: unity-cli의 playmode-verification-loop 절차대로 `editor_play` → 프레임 진행 확인 → 콘솔 에러 확인을 한 뒤, 키 입력이 필요한 다음 항목은 오준석님이 실행 중인 에디터에서 직접 조작해 확인한다. 끝나면 `editor_stop`.
>
> **확인 방식 변경(1.2 이후 오준석님 결정)**: 2~6단계의 체크리스트는 `eval`로 `Player`의 `IPlayerInput`을 가짜 입력으로 바꿔 주입하고, 위치·스케일·상태 전이·애니메이터 파라미터·`AudioSource` 재생을 읽어 자동 확인한다. 오준석님의 직접 조작 확인은 7.2에서 한 번 한다. 새 Input System 전환은 이번 변경이 끝난 뒤 별도 변경으로 진행한다.
>
> ① ←/→로 이동하고 방향에 따라 캐릭터가 좌우 반전된다 ② 키를 떼면 idle 애니메이션 ③ ↑로 점프하고 공중에서 좌우 이동이 된다 ④ 착지하면 입력에 따라 idle/run으로 돌아온다 ⑤ Space로 공격 애니메이션과 gunfire 소리가 나고 끝나면 idle/run으로 돌아온다

## 0. 작업 브랜치와 에디터 연결 준비

- [x] 0.0 `refactoring` 브랜치에서 작업 브랜치 `refactor/player-srp`를 만들고(`git switch -c refactor/player-srp`) `git branch --show-current`로 전환됐는지 확인한다
- [x] 0.1 `unity status`로 이 프로젝트의 Editor가 `ready`인지, `unity command`로 `recompile`·`editor_play`·`eval`이 노출되는지 확인한다. 연결되지 않으면 `unity pipeline list`로 Safe Mode 여부를 확인하고, 패키지가 없으면 오준석님께 확인 후 `unity pipeline install`을 실행한다
- [x] 0.2 `unity command set_autotick`을 실행하고 컴파일 확인을 한 번 통과시켜 기준 상태(에러 없음)를 확인한다

## 1. 입력 분리

- [x] 1.1 `Assets/scripts/Player/IPlayerInput.cs`(Horizontal, JumpPressed, AttackPressed)와 `UnityPlayerInput.cs`(←/→, ↑, Space, ← 우선)를 추가하고 컴파일 확인을 통과시킨다
- [x] 1.2 `Player`가 `IPlayerInput`을 생성해 사용하도록 바꾸고 `ReadInput()`, `MoveInput`/`JumpPressed` 필드, `Update`의 `Input.GetKeyDown(KeyCode.Space)`를 제거한 뒤, `Player.cs`에 `UnityEngine.Input` 호출이 남지 않았는지 grep으로 확인하고 체크리스트를 통과시킨다

## 2. 애니메이터 분리

- [x] 2.1 `PlayerAnimator`(해시 상수, `SetWalking`, `SetGrounded`, `PlayAttack`)를 추가하고 `Player`의 `_animator.SetBool/SetTrigger` 호출을 모두 위임으로 바꾼 뒤, `"Walk"`, `"Grounded"`, `"attack"` 문자열이 `PlayerAnimator.cs`에만 남았는지 grep으로 확인하고 체크리스트를 통과시킨다

## 3. 이동 분리

- [x] 3.1 `PlayerMotor`(`Move(direction)`, `Jump()`)를 추가하고 `Player.Move/Jump` 본문을 옮긴 뒤, `MoveSpeed`(6)·`JumpForce`(500)를 `Player`의 `[SerializeField]` 필드로 바꿔 생성자로 넘긴다. `unity command eval`로 씬의 Player 컴포넌트 값이 6/500으로 읽히는지, 체크리스트 ①③이 이전과 같은 속도·높이로 동작하는지 확인한다

## 4. 착지 판정 분리

- [x] 4.1 `GroundDetector`(`OnContactEnter/Exit(Collider2D)`, `IsGrounded`, `GroundedChanged` 이벤트, `Ground` 태그 판정)를 추가하고, `Player`의 충돌 콜백은 위임만 하며 `GroundedChanged`를 `PlayerAnimator.SetGrounded`에 연결한다. `CompareTag("Ground")`가 `GroundDetector.cs`에만 있는지 확인하고 체크리스트 ③④를 통과시킨다

## 5. 사운드·공격 분리

- [x] 5.1 `PlayerAudio`(`PlayGunfire()`, clip이 null이면 무시)를 추가하고, `_gunfireClip` 필드 이름을 유지한 채 생성자로 넘긴다. 컴파일 확인 후 `unity command eval`로 씬의 Player `_gunfireClip`이 여전히 `gunfire` 클립을 가리키는지 확인한다
- [x] 5.2 `PlayerWeapon.Fire()`(`PlayAttack` + `PlayGunfire`)를 추가하고 `Player.Update`에서 `StateMachine.Update()` 다음에 `if (input.AttackPressed) weapon.Fire();`로 호출한 뒤 체크리스트 ⑤를 통과시킨다

## 6. 상태가 인터페이스에만 의존하게 하기

- [x] 6.1 `IPlayerContext`(design.md D3)를 추가하고 `Player`가 구현하게 한다. 각 멤버는 부품 위임만 하는지 확인한다
- [x] 6.2 `PlayerState`와 Idle/Move/Jump 상태가 `IPlayerContext`만 받도록 바꾸고 전이를 `context.ChangeState(...)`로 통일한 뒤, `PlayerStates/` 폴더에 `Player`, `StateMachine`, `UnityEngine` 참조가 없는지 grep으로 확인하고 체크리스트 전체를 통과시킨다

## 7. 정리와 통합 확인

- [ ] 7.1 `Player.cs`에 부품 조립(Awake), 상태머신 구동(Update/FixedUpdate), 충돌 위임, `IPlayerContext` 위임 외의 로직이 남지 않았는지 코드로 확인하고 불필요한 `public` 프로퍼티(`StateMachine` 등)를 내부로 숨긴다
- [ ] 7.2 컴파일 확인으로 Editor가 새 `.cs` 파일의 `.meta`를 생성하게 한 뒤, `git status`로 새 `.cs` 파일마다 `.meta`가 함께 있는지, `Main.unity`·`Player.controller`가 변경되지 않았는지 확인한 뒤 체크리스트 전체를 한 번 더 통과시킨다

## Workflow follow-up

- 모든 작업이 끝나면 머지·푸시·PR 생성을 하지 않고, `refactor/player-srp` 브랜치의 변경 요약(`git diff refactoring...refactor/player-srp --stat`)을 보고한 뒤 오준석님의 리뷰를 기다린다.
- 리뷰 후 반영과 아카이브(`/opsx:archive`)는 오준석님의 요청이 있을 때 진행한다.
