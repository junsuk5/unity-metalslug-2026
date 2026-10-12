# Proposal

## Why

`refactor-player-srp`에서 Player를 부품으로 나눴지만 자동화 테스트가 없다. 그래서 매 단계의 회귀 확인을 `eval`로 가짜 입력을 주입하는 임시 스크립트와 수동 조작에 의존했다. 이 스크립트는 리플렉션으로 `Player`의 숨은 backing field를 찾아 바꿔 끼우는 방식이라, 필드 이름만 바뀌어도 깨진다. 기능(총알 발사, 앉기, 낙하 등)이 더 붙기 전에 상태 전이 규칙과 Player 조립을 지키는 테스트를 만들어 두려는 것이다.

## What Changes

- 게임 스크립트(`Assets/scripts/`)를 런타임 어셈블리 정의(asmdef)로 묶고, EditMode·PlayMode 테스트 어셈블리를 추가한다.
- **EditMode 단위 테스트**를 추가한다.
  - 상태(Idle/Move/Jump) 전이 규칙: 직접 짠 가짜 `IPlayerContext`·`IPlayerInput` 사용
  - `PlayerMotor`(방향 전환·이동량·점프 전 속도 초기화)
  - `GroundDetector`(`Ground` 태그 판정, `GroundedChanged` 이벤트)
- **PlayMode 통합 테스트**를 추가한다. 실제 `Main` 씬의 Player에 가짜 입력을 주입해 이동·정지·점프·착지·공격이 끝에서 끝까지 동작하는지, 즉 Player 조립이 맞는지 확인한다.
- 테스트를 위해 동작을 바꾸지 않는 범위에서 프로덕션 코드를 조금 고친다.
  - `PlayerMotor.Move`가 `deltaTime`을 인자로 받는다. `Player`는 `Time.deltaTime`을 넘긴다.
  - `Player`에 테스트 전용 `internal` 입력 주입 지점(`SetInput`)과 현재 상태 조회(`CurrentState`)를 추가하고, `InternalsVisibleTo`로 PlayMode 테스트 어셈블리에만 공개한다.
- mock 라이브러리(NSubstitute 등)는 도입하지 않는다. 지금은 인터페이스가 `IPlayerInput`·`IPlayerContext` 둘뿐이라 직접 짠 가짜 객체로 충분하다.
- 게임 플레이 동작, 씬, 애니메이터, Build Settings는 바꾸지 않는다.

## Capabilities

### New Capabilities
<!-- 없음: 게임 동작을 바꾸지 않는 테스트·도구 추가이므로 skip_specs: true -->

### Modified Capabilities
<!-- 없음 -->

## Impact

- 추가: `Assets/scripts/MetalSlug.asmdef`, `Assets/scripts/AssemblyInfo.cs`, `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/` (각각 asmdef와 테스트 코드)
- 수정: `Assets/scripts/Player/PlayerMotor.cs`, `Assets/scripts/Player.cs`
- 어셈블리 이동: `Assets/scripts/` 아래 모든 스크립트(`Enemy`, `Bullet`, `FollowCam`, `test` 포함)가 `Assembly-CSharp`에서 `MetalSlug` 어셈블리로 옮겨진다. 씬은 스크립트를 GUID로 참조하므로 참조는 유지되어야 하며, 이를 검증한다(design.md 참고).
- 패키지: 이미 설치된 `com.unity.test-framework` 1.6.0을 사용하고 새 패키지는 추가하지 않는다.
- 선행 조건: `refactor-player-srp`(브랜치 `refactor/player-srp`, 리뷰 대기 중) 위에서 작업한다.
- 범위 밖: 새 Input System 전환(별도 변경), `Enemy`·`Bullet` 테스트, CI 구성, `UnityPlayerInput`·`PlayerAnimator`·`PlayerAudio`·`PlayerWeapon` 단위 테스트(레거시 Input·Animator·AudioSource를 감싸기만 하는 클래스라 PlayMode 통합 테스트에서 다룬다).
