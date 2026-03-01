# Timeline Action System

Unity 기반 3D 액션 게임의 **전투 시스템 핵심 아키텍처** 소스코드입니다.
ZZZ(젠레스 존 제로) 스타일의 스타일리시 액션 전투를 목표로 설계했습니다.

> 이 레포지토리는 전체 Unity 프로젝트가 아닌, 핵심 시스템 소스코드만 발췌한 포트폴리오용 레포입니다.

---

## 핵심 시스템

### 1. Timeline Action — 데이터 드리븐 액션 시스템

전투 액션을 **코드가 아닌 Unity Timeline 에셋**으로 정의합니다.

```
ActionSO (TimelineAsset 상속)
├── AnimationTrack    → 애니메이션 클립
├── NotifyState Track → 구간 이벤트 (히트박스, 회피 판정, 카메라 연출 등)
├── Notify Marker     → 단발 이벤트 (카메라 셰이크, 에너지 소모 등)
└── EnterTrack        → 액션 진입 시 즉시 실행되는 이벤트
```

**왜 이렇게 만들었는가?**

기존 Animator StateMachineBehaviour 기반에서는 공격 타이밍, 히트박스 활성 구간, 캔슬 윈도우 등을 전부 코드에 하드코딩해야 했습니다. 액션이 늘어날수록 유지보수가 불가능해지는 문제가 있었고, 이를 해결하기 위해 **Timeline 에셋 하나에 액션의 모든 정보를 담는 데이터 드리븐 구조**로 전환했습니다.

`ActionUnpacker`가 Timeline 에셋을 파싱해서 Notify/NotifyState 리스트로 변환하고, `ActionCtrl`이 매 프레임 시간 기반으로 이벤트를 순차 실행합니다. Unity의 PlayableGraph를 사용하지 않고 **직접 시간을 추적**하는 방식을 선택한 이유는, PlayableGraph의 이벤트 발화 타이밍이 불안정하고, 액션 전환 시 즉시 중단/재시작이 어려웠기 때문입니다.

### 2. 커맨드 전환 시스템 — 입력 기반 액션 체이닝

```
CommandTransitionInfo
├── command      → 어떤 입력인지 (BaseAttack, Dodge, SpecialAttack...)
├── phase        → Down / Up / Press
├── modifier     → Hold 조건 (길게 누르기 체크)
└── actionName   → 전환할 액션
```

각 액션(ActionSO)은 자신이 받아들일 수 있는 **커맨드 전환 목록**을 가지고 있습니다. 예를 들어 `Attack_Normal_01`은 BaseAttack 입력을 받으면 `Attack_Normal_02`로 전환되고, Dodge를 받으면 `Evade_Front`로 전환됩니다. **어떤 액션에서 어떤 입력으로 어떤 액션으로 갈 수 있는지가 전부 데이터**입니다.

`CommandTransitionNotifyState`는 이 전환이 유효한 **시간 구간**을 Timeline 위에서 시각적으로 설정할 수 있게 해줍니다. 즉, "공격 모션의 후반 30%에서만 다음 공격으로 캔슬 가능"같은 설정을 코드 없이 가능하게 합니다.

**입력 버퍼** 구현도 포함되어 있어, 캔슬 윈도우가 열리기 직전에 들어온 입력도 소비할 수 있습니다.

### 3. 히트박스 / 허트박스 시스템

`BoxNotifyState`로 Timeline 위에 히트박스의 활성 구간, 형태(Sphere/Box), 위치, 대미지, 히트스톱, 넉백, 히트 이펙트/사운드를 모두 설정합니다.

```
BoxNotifyState
├── boxType       → HitBox / DodgeBox
├── boxShape      → Sphere / Box
├── center, radius, size
├── hitStrength   → Low / High
├── hitstopDuration, hitstopSpeed
├── knockbackDuration, hitGatherDist
└── particlePrefab, hitAudio
```

`SimpleAttackProcess`가 매 프레임 Physics.OverlapSphereNonAlloc / OverlapBoxNonAlloc으로 충돌을 검사하고, 중복 히트 방지를 위해 HashSet으로 이미 맞은 대상을 추적합니다.

### 4. 파티 시스템과 캐릭터 교체

ZZZ처럼 **다중 캐릭터 파티**를 운용합니다.

- **일반 교체**: 현재 캐릭터가 SwitchOut, 다음 캐릭터가 SwitchIn 액션 재생
- **패리 어시스트**: 몬스터의 공격 판정(DodgeBox) 도중 교체하면 **패리 어시스트** 발동 — 교체되어 들어오는 캐릭터가 몬스터 앞에 텔레포트하며 반격 모션 진입

```
교체 시 DodgeBox 활성 감지?
├── Yes → SwitchInParryAid (패리 어시스트)
│         ├── 몬스터 앞 텔레포트
│         ├── 패리 성공 VFX/SFX
│         └── 포스트 프로세싱 연출
└── No  → SwitchInNormal (일반 교체)
```

### 5. 카메라 연출 시스템

Cinemachine 기반으로 여러 카메라 모드를 전투 상황에 따라 전환합니다.

| 모드 | 동작 | 사용처 |
|------|------|--------|
| **Tracking** | 매 프레임 캐릭터 forward를 POV yaw가 추적 | 일반 공격 |
| **SnapLock** | 시전 시점에 카메라 yaw 스냅 + Follow/LookAt 고정 | 궁극기, 강공격 |
| **AssistCamera** | 좌/우 어시스트 VCam 중 가까운 쪽 활성화 | 패리 어시스트 |
| **TeleportBehind** | DOTween으로 POV yaw를 궤도 보간 | 적 후방 텔레포트 |
| **UltimateCamera** | 전용 VCam + Culling Mask + Skybox 교체 + 시네마틱 바 | 궁극기 연출 |

`GameInputCameraProvider`는 카메라 전환 시 **입력 억제 → 블렌드 대기 → 점진적 복원** 파이프라인으로 전환 중 마우스 입력이 카메라를 튀게 하는 문제를 해결합니다.

### 6. 몬스터 전투 AI

`SimpleMonsterCtrl`은 간단하지만 핵심적인 전투 행동을 구현합니다.

- **슈퍼아머**: 연속 피격 임계값 도달 시 경직 무시 + 반격 전환
- **히트 리액션**: 공격 방향(Front/Back/Left/Right) 기반 4방향 리액션 + 강도별(Low/High) 분기
- **넉백**: ease-out 감쇠 적용, 슈퍼아머 중에도 적용
- **공격 AI**: 범위 내 캐릭터 탐색 → 회전 → 공격 액션 실행

---

## 설계 고민

### Timeline을 왜 런타임에서 직접 파싱하는가?

Unity Timeline은 본래 **시네마틱 도구**입니다. 하지만 Track/Clip/Marker 구조가 액션 게임의 "구간 이벤트 + 단발 이벤트"와 정확히 일치합니다. Timeline Editor의 **시각적 편집** 능력을 활용하되, 런타임에서는 PlayableGraph 대신 직접 시간 기반 순회를 하는 **하이브리드 접근**을 택했습니다.

### 입력 버퍼는 왜 필요한가?

액션 게임에서 "캔슬 윈도우가 열리기 0.1초 전에 누른 버튼"도 의도된 입력입니다. `Deque<InputBufferItem>` 기반 시간 윈도우 방식으로, 캔슬 윈도우가 열리는 순간 버퍼에서 유효한 입력을 소비합니다. 이를 통해 격투 게임 수준의 입력 반응성을 확보했습니다.

### 카메라 입력 억제/복원은 왜 필요한가?

Cinemachine VCam 전환 시 POV 입력이 즉시 적용되면, 블렌딩 중 카메라가 의도치 않은 방향으로 튑니다. `GameInputCameraProvider`의 **Suppress → WaitBlend → Restore** 3단계 FSM으로 블렌딩이 완전히 끝난 후 입력을 점진적으로 복원합니다.

---

## 기술 스택

- **Unity** (C#)
- **Unity Timeline** — 액션 데이터 에디팅
- **Cinemachine** — 카메라 연출
- **DOTween** — 트윈 애니메이션
- **UniTask** — async/await 기반 비동기 처리
- **Unity Input System** — 입력 처리

---

## 프로젝트 구조

```
├── GameInput.cs                          # Input System 자동 생성 래퍼
├── Scripts/
│   ├── Cinemachine/                      # 카메라 제어
│   │   ├── GameInputCameraProvider.cs    # POV 입력 억제/복원 FSM
│   │   ├── CinemachineZoomController.cs  # 줌 제어
│   │   └── CinemachineImpulseController.cs # 카메라 셰이크
│   ├── Manager/                          # 오디오, UI 매니저
│   ├── Monster/
│   │   └── SimpleMonsterCtrl.cs          # 몬스터 AI + 슈퍼아머 + 히트 리액션
│   ├── Role/
│   │   ├── SimpleRoleCtrl.cs             # 플레이어 캐릭터 제어 + 카메라 연출
│   │   └── ChGravity.cs                  # 중력 처리
│   ├── SceneManager/
│   │   └── SimpleAttackProcess.cs        # 히트 판정 + 대미지 처리
│   ├── System/
│   │   └── SimplePlayerSystem.cs         # 파티 관리 + 입력 라우팅 + 캐릭터 교체
│   ├── TimelineAction/Runtime/           # 핵심 액션 시스템
│   │   ├── ActionCtrl.cs                 # 액션 재생기 + 커맨드 전환 + 입력 버퍼
│   │   ├── ActionUnpacker.cs             # Timeline → Notify/NotifyState 변환
│   │   ├── ActionSO.cs             # 액션 데이터 정의 (TimelineAsset 상속)
│   │   ├── ActionNotify/                 # 단발 이벤트 베이스
│   │   ├── ActionNotifyState/            # 구간 이벤트 베이스
│   │   └── BuiltInNotifyState/           # 커맨드 전환 NotifyState
│   ├── TimelineActionCustom/             # 게임 전용 Notify/NotifyState 구현
│   │   ├── CustomNotify/                 # 카메라 셰이크, 히트스톱, 에너지 소모 등
│   │   └── CustomNotifyState/            # 히트박스, 회피, 패리, 텔레포트, 궁극기 카메라 등
│   └── Utils/                            # LiveData, Singleton, 오브젝트 풀 등
```
