# Proposal

## Why

`Player.cs` 하나가 입력 읽기, 이동·점프 물리, 착지 판정, 애니메이터 파라미터, 사운드 재생, 공격, 상태머신 소유까지 7가지 책임을 동시에 지고 있다. 상태 클래스(`PlayerIdleState` 등)도 구체 클래스 `Player`에 직접 의존해서, 상태 전이 로직만 따로 이해하거나 바꾸기 어렵다. FSM을 막 도입한 지금이 기능(총알 발사, 앉기, 낙하 등)이 더 붙기 전에 책임을 나눌 적기다.

## What Changes

- 입력 읽기를 `IPlayerInput` 인터페이스와 `UnityPlayerInput` 구현으로 분리한다. 공격 입력도 여기로 모은다.
- 이동·방향 전환·점프(Transform/Rigidbody2D 조작)를 `PlayerMotor`로 분리한다.
- `Ground` 태그 충돌 기반 착지 판정을 `GroundDetector`로 분리한다.
- 애니메이터 파라미터(`Walk`, `Grounded`, `attack`) 설정을 `PlayerAnimator`로 모으고 문자열 대신 해시를 사용한다.
- 사운드 재생을 `PlayerAudio`로, 공격(애니메이션 + 사운드)을 `PlayerWeapon`으로 분리한다.
- 상태 클래스가 `Player` 대신 `IPlayerContext` 인터페이스에만 의존하도록 바꾼다.
- `Player`는 위 부품을 조립하고 상태머신을 구동하는 역할만 남긴다.
- 게임 플레이 동작(이동 속도 6, 점프 힘 500, 키 배치, 애니메이션, gunfire 사운드)은 그대로 유지한다. 외부에서 보이는 동작 변화는 없다.

## Capabilities

### New Capabilities
<!-- 없음: 동작을 바꾸지 않는 순수 리팩토링이므로 skip_specs: true -->

### Modified Capabilities
<!-- 없음 -->

## Impact

- 수정: `Assets/scripts/Player.cs`, `Assets/scripts/PlayerStates/*.cs`
- 추가: `Assets/scripts/Player/` 아래 입력·모터·착지 판정·애니메이터·오디오·무기 클래스와 `IPlayerContext`
- 씬: `Assets/Scenes/Main.unity`의 Player에 연결된 `_gunfireClip` 참조가 유지되어야 한다. 새 클래스는 MonoBehaviour가 아닌 일반 C# 클래스로 만들어 씬에 컴포넌트를 추가하지 않는다(design.md 참고).
- `FSM` 네임스페이스(`IState`, `StateMachine`), `Player.controller`, 다른 스크립트(`Enemy`, `Bullet`, `FollowCam`)는 변경하지 않는다.
- 자동화 테스트 추가는 이번 범위에서 제외한다(테스트 어셈블리를 위해 asmdef 도입이 필요해 영향 범위가 커짐). 대신 인터페이스 분리로 이후 테스트가 가능한 구조를 만든다.
