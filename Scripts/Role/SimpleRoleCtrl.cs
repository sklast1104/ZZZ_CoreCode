using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JM.TimelineAction;

namespace JM
{
    public class SimpleRoleCtrl : MonoBehaviour
    {
        public Transform vcamFollow;
        public Transform vcamLookAt;

        public CinemachineVirtualCamera leftAssistVCam;
        public CinemachineVirtualCamera rightAssistVCam;

        [Header("Ultimate Mode")]
        public CinemachineVirtualCamera ultimateVCam;
        public GameObject ultimateDomePrefab;
        [SerializeField] private float domeScale = 15f;
        public Material ultimateSkyboxMat;

        [Header("HP")]
        [SerializeField] private float maxHp = 10000f;
        public float CurrentHp { get; private set; }
        public float MaxHp => maxHp;

        public float inputRotationSpeed = 15f;
        public float lookAtMonsterRotationSpeed = 20f;
        private Quaternion TargetRotation { get; set; }

        public CinemachineVirtualCamera VCam { get; set; }
        public Transform Cam { get; private set; }
        public ActionCtrl ActionCtrl { get; private set; }
        private Animator Animator { get; set; }
        private CharacterController CharacterController { get; set; }
        private ChGravity ChGravity { get; set; }

        private bool EnableRotation { get; set; }
        private bool EnableRecenter { get; set; }
        private bool EnableLookAtMonster { get; set; }
        private ECameraFollowForwardMode CameraFollowForwardMode { get; set; }
        private float lockedCameraYaw;
        private Vector3 lockedFollowPos;
        private Vector3 lockedLookAtPos;
        private Vector3 savedFollowLocalPos;
        private Vector3 savedLookAtLocalPos;
        public bool DisableMonsterPush { get; private set; }
        private bool IsUltimateMode;
        private GameObject _activeDome;
        private int _savedCullingMask;
        private CameraClearFlags _savedClearFlags;
        private Color _savedBgColor;
        private Material _savedSkybox;
        private CinemachineBlendDefinition.Style _savedBlendStyle;
        private float _savedBlendTime;

        public bool IsAttacking { get; set; }
        public bool IsParrying { get; set; }

        public GameInput GameInput { get; set; }

        private BoxNotifyState activeHitBox;
        private readonly HashSet<SimpleMonsterCtrl> hitMonsters = new();

        private Renderer[] _renderers;

        private void Awake()
        {
            CurrentHp = maxHp;
            Cam = Camera.main.transform;
            Animator = GetComponent<Animator>();
            _renderers = GetComponentsInChildren<Renderer>();

            CharacterController = GetComponent<CharacterController>();
            ChGravity = GetComponent<ChGravity>();

            ActionCtrl = GetComponent<ActionCtrl>();
            if (ActionCtrl != null)
            {
                ActionCtrl.IsActionExecutable = IsActionExecutable;
                ActionCtrl.OnExecuteAction = OnExecuteAction;
                ActionCtrl.OnSetActionArgs = SetActionArgs;
            }

            if (leftAssistVCam != null) leftAssistVCam.enabled = false;
            if (rightAssistVCam != null) rightAssistVCam.enabled = false;
            if (ultimateVCam != null) ultimateVCam.enabled = false;
        }

        private void OnExecuteAction(ActionSO action, string actionName)
        {
            Debug.Log($"[SimpleRoleCtrl] ExecuteAction: {actionName} (type: {action.actionType})");
        }

        private bool IsActionExecutable(ActionSO action)
        {
            return true;
        }

        private void Update()
        {
            UpdateRotation();
            UpdateRecenter();
            UpdateLookAtMonster();
            UpdateCameraFollowForward();
            UpdateHitDetection();
        }

        private void SetActionArgs(ActionArgs actionArgs)
        {
            EnableRotation = actionArgs.enableRotation;
            EnableRecenter = actionArgs.enableRecenter;
            EnableLookAtMonster = actionArgs.enableLookAtMonster;
            DisableMonsterPush = actionArgs.disableMonsterPush;
            SetShowState(actionArgs.roleShowState);

            var prevMode = CameraFollowForwardMode;
            CameraFollowForwardMode = actionArgs.cameraFollowForwardMode;

            // 모드 진입
            if (CameraFollowForwardMode != ECameraFollowForwardMode.Disabled
                && prevMode == ECameraFollowForwardMode.Disabled)
            {
                var provider = VCam != null ? VCam.GetComponent<GameInputCameraProvider>() : null;
                if (provider != null) provider.SuppressInput();

                if (CameraFollowForwardMode == ECameraFollowForwardMode.SnapLock)
                {
                    // 가장 가까운 몬스터 방향으로 캐릭터 회전
                    Vector3 faceDir = Vector3.zero;
                    float closestDist = float.MaxValue;

                    if (MonsterSystem.Instance != null)
                    {
                        var m = MonsterSystem.Instance.ClosestMonster(transform.position, out float d);
                        if (m != null && d < closestDist) { closestDist = d; faceDir = m.transform.position - transform.position; }
                    }
                    foreach (var sm in SimpleMonsterCtrl.AllMonsters)
                    {
                        float d = Vector3.Distance(transform.position, sm.transform.position);
                        if (d < closestDist) { closestDist = d; faceDir = sm.transform.position - transform.position; }
                    }

                    faceDir.y = 0f;
                    if (faceDir.sqrMagnitude < 0.001f)
                        faceDir = transform.forward;
                    faceDir.Normalize();

                    transform.rotation = Quaternion.LookRotation(faceDir);

                    // POV yaw를 캐릭터 등 뒤로 스냅
                    var pov = VCam != null ? VCam.GetCinemachineComponent<CinemachinePOV>() : null;
                    if (pov != null)
                    {
                        lockedCameraYaw = Quaternion.LookRotation(faceDir).eulerAngles.y;
                        pov.m_HorizontalAxis.Value = lockedCameraYaw;
                    }

                    // Follow/LookAt 로컬 위치 저장 후, 월드 위치 고정 (LateUpdate에서 유지)
                    if (vcamFollow != null)
                    {
                        savedFollowLocalPos = vcamFollow.localPosition;
                        lockedFollowPos = vcamFollow.position;
                    }
                    if (vcamLookAt != null)
                    {
                        savedLookAtLocalPos = vcamLookAt.localPosition;
                        lockedLookAtPos = vcamLookAt.position;
                    }
                }
            }
            // 모드 해제
            else if (CameraFollowForwardMode == ECameraFollowForwardMode.Disabled
                     && prevMode != ECameraFollowForwardMode.Disabled)
            {
                // Follow/LookAt 로컬 위치 복원
                if (vcamFollow != null)
                    vcamFollow.localPosition = savedFollowLocalPos;
                if (vcamLookAt != null)
                    vcamLookAt.localPosition = savedLookAtLocalPos;

                var provider = VCam != null ? VCam.GetComponent<GameInputCameraProvider>() : null;
                if (provider != null) provider.BeginRestore(0.3f);
            }

            // Ultimate Mode
            bool prevUlt = IsUltimateMode;
            bool curUlt = actionArgs.ultimateMode;
            if (curUlt && !prevUlt) EnterUltimateMode();
            else if (!curUlt && prevUlt) ExitUltimateMode();
        }

        private void UpdateRecenter()
        {
            if (VCam == null) return;
            var pov = VCam.GetCinemachineComponent<CinemachinePOV>();
            if (pov == null) return;

            bool enableRecenter = false;
            if (EnableRecenter && GameInput != null)
            {
                var inputDir = GameInput.Combat.Move.ReadValue<Vector2>();
                if (inputDir != Vector2.zero)
                {
                    float angle = Vector2.Angle(inputDir, Vector2.up);
                    if (angle is > 10f and < 170f)
                    {
                        enableRecenter = true;
                    }
                }
            }
            pov.m_HorizontalRecentering.m_enabled = enableRecenter;
        }

        private void UpdateRotation()
        {
            if (!EnableRotation || GameInput == null || VCam == null) return;

            var inputDir = GameInput.Combat.Move.ReadValue<Vector2>();
            if (inputDir == Vector2.zero) return;

            var camForward = VCam.transform.forward;
            var camRight = VCam.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
            var wsMoveDir = camForward * inputDir.y + camRight * inputDir.x;
            var rot = Quaternion.LookRotation(wsMoveDir);

            transform.DOKill();
            transform.DORotateQuaternion(rot, 0.1f);
        }

        public async UniTaskVoid TriggerHitstop(float duration, float speed)
        {
            if (duration <= 0f) return;

            ActionCtrl.Speed = speed;
            await UniTask.WaitForSeconds(duration);
            ActionCtrl.Speed = 1f;
        }

        private void OnAnimatorMove()
        {
            AddPos(Animator.deltaPosition);

            // SnapLock 중에는 deltaRotation 적용 차단
            // → Humanoid 리타게팅/CrossFade 블렌딩에서 발생하는 미세한 회전 누적 방지
            if (CameraFollowForwardMode != ECameraFollowForwardMode.SnapLock)
                transform.rotation *= Animator.deltaRotation;
        }

        private void UpdateLookAtMonster()
        {
            if (!EnableLookAtMonster) return;

            float maxDist = 15f;
            Transform target = null;
            float dist = float.MaxValue;

            if (MonsterSystem.Instance != null)
            {
                var monster = MonsterSystem.Instance.ClosestMonster(transform.position, out dist);
                if (monster != null) target = monster.transform;
            }

            foreach (var sm in SimpleMonsterCtrl.AllMonsters)
            {
                float d = Vector3.Distance(transform.position, sm.transform.position);
                if (d < dist) { dist = d; target = sm.transform; }
            }

            if (target == null || dist >= maxDist) return;

            var dir = target.position - transform.position;
            dir.y = 0;
            TargetRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation,
                Time.deltaTime * lookAtMonsterRotationSpeed);
        }

        private void UpdateCameraFollowForward()
        {
            if (CameraFollowForwardMode == ECameraFollowForwardMode.Disabled) return;
            if (VCam == null) return;

            var pov = VCam.GetCinemachineComponent<CinemachinePOV>();
            if (pov == null) return;

            switch (CameraFollowForwardMode)
            {
                case ECameraFollowForwardMode.Tracking:
                {
                    Vector3 fwd = transform.forward;
                    fwd.y = 0f;
                    if (fwd.sqrMagnitude < 0.001f) return;

                    float targetYaw = Quaternion.LookRotation(fwd).eulerAngles.y;
                    float currentYaw = pov.m_HorizontalAxis.Value;
                    float delta = Mathf.DeltaAngle(currentYaw, targetYaw);
                    pov.m_HorizontalAxis.Value = Mathf.Lerp(currentYaw, currentYaw + delta, Time.deltaTime * 10f);
                    break;
                }
                case ECameraFollowForwardMode.SnapLock:
                {
                    pov.m_HorizontalAxis.Value = lockedCameraYaw;
                    break;
                }
            }
        }

        private void LateUpdate()
        {
            // SnapLock: Follow/LookAt 타겟을 시전 시점 위치로 고정 → 카메라 완전 정지
            if (CameraFollowForwardMode == ECameraFollowForwardMode.SnapLock)
            {
                if (vcamFollow != null)
                    vcamFollow.position = lockedFollowPos;
                if (vcamLookAt != null)
                    vcamLookAt.position = lockedLookAtPos;
            }
        }

        public void InitFront()
        {
            if (ActionCtrl == null) ActionCtrl = GetComponent<ActionCtrl>();
            if (ActionCtrl != null) ActionCtrl.PlayAction(ActionName.Idle);
        }

        public void InitBackground()
        {
            if (ActionCtrl == null) ActionCtrl = GetComponent<ActionCtrl>();
            if (ActionCtrl != null) ActionCtrl.PlayAction(ActionName.Background);
        }

        public void SetPos(Vector3 pos)
        {
            if (CharacterController.enabled)
            {
                CharacterController.enabled = false;
                transform.position = pos;
                CharacterController.enabled = true;
            }
            else
            {
                transform.position = pos;
            }
        }

        public void AddPos(Vector3 delta)
        {
            if (CharacterController.enabled)
            {
                CharacterController.Move(delta);
            }
            else
            {
                transform.position += delta;
            }
        }

        public void SwitchInParryAid(MonsterCtrl monster)
        {
            Debug.Log($"{name} SwitchInParryAid");
            const float parryDist = 2.5f;
            transform.forward = -monster.transform.forward;
            SetPos(monster.transform.position + monster.transform.forward * parryDist);
            ActionCtrl.PlayAction(ActionName.Attack_ParryAid_Start);
        }

        public void SwitchInParryAid(SimpleMonsterCtrl monster)
        {
            Debug.Log($"{name} SwitchInParryAid");
            const float parryDist = 2.5f;
            transform.forward = -monster.transform.forward;
            SetPos(monster.transform.position + monster.transform.forward * parryDist);
            monster.ParryingRole = this;
            ActionCtrl.PlayAction(ActionName.Attack_ParryAid_Start);
        }

        private Vector3 switchNextLocalPos = new(1, 0, -1);
        private Vector3 switchPrevLocalPos = new(-1, 0, -1);

        private void SetShowState(ERoleShowState state)
        {
            switch (state)
            {
                case ERoleShowState.Front:
                    ChGravity.enabled = true;
                    CharacterController.enabled = true;
                    SetRenderersVisible(true);
                    break;
                case ERoleShowState.Ghost:
                    ChGravity.enabled = false;
                    CharacterController.enabled = false;
                    SetRenderersVisible(true);
                    break;
                case ERoleShowState.Background:
                    ChGravity.enabled = false;
                    CharacterController.enabled = false;
                    SetRenderersVisible(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void SetRenderersVisible(bool visible)
        {
            foreach (var r in _renderers)
                if (r != null) r.enabled = visible;
        }

        public void SwitchInNormal(SimpleRoleCtrl prev, bool isNext)
        {
            transform.forward = prev.transform.forward;
            SetPos(prev.transform.position);
            ActionCtrl.PlayAction(ActionName.SwitchIn_Normal);
        }

        public void ShouldSwitchOutNormal()
        {
            transform.DOKill();
            IsAttacking = false;
            ActionCtrl.PlayAction(ActionName.SwitchOut_Normal);
        }

        public void ShouldSwitchOutAided()
        {
            if (!IsAttacking)
            {
                ActionCtrl.PlayAction(ActionName.Background);
            }
        }

        public async UniTaskVoid EnterAttackParryAid()
        {
            Debug.Log($"{name} EnterAttackParryAid");
            ActionCtrl.PlayAction(ActionName.Attack_ParryAid_L);
            const float duration = 0.5f;
            ActionCtrl.Speed = 0f;
            await UniTask.WaitForSeconds(duration);
            ActionCtrl.Speed = 1f;
        }

        public void BoxBegin(BoxNotifyState box)
        {
            if (box.boxType == EBoxType.HitBox)
            {
                activeHitBox = box;
                hitMonsters.Clear();
                SimpleAttackProcess.RoleAttack(this, box, hitMonsters);
            }
        }

        public void BoxEnd(BoxNotifyState box)
        {
            if (activeHitBox == box)
            {
                activeHitBox = null;
                hitMonsters.Clear();
            }
        }

        private void UpdateHitDetection()
        {
            if (activeHitBox != null)
                SimpleAttackProcess.RoleAttack(this, activeHitBox, hitMonsters);
        }

        public void HandleMonsterAttackRole(float damage, Vector3 attackerPos, EHitStrength hitStrength)
        {
            if (IsParrying) return;

            // Determine front/back based on attacker position relative to character facing
            var toAttacker = (attackerPos - transform.position).normalized;
            toAttacker.y = 0;
            float dot = Vector3.Dot(transform.forward, toAttacker);
            bool isFront = dot >= 0f;

            string prefix = hitStrength == EHitStrength.High ? "Hit_H" : "Hit_L";
            string dir = isFront ? "Front" : "Back";
            string actionName = $"{prefix}_{dir}";

            Debug.Log($"[SimpleRoleCtrl] {name} hit! damage:{damage} strength:{hitStrength} dir:{dir} action:{actionName}");

            if (ActionCtrl.TryGetAction(actionName, out _))
            {
                ActionCtrl.PlayAction(actionName);
            }
            else if (ActionCtrl.TryGetAction(ActionName.Hit_L_Front, out _))
            {
                ActionCtrl.PlayAction(ActionName.Hit_L_Front);
            }
        }

        public void EnterAssistCamera()
        {
            if (leftAssistVCam == null || rightAssistVCam == null) return;

            float leftDist = Vector3.Distance(
                leftAssistVCam.transform.position, Cam.position);
            float rightDist = Vector3.Distance(
                rightAssistVCam.transform.position, Cam.position);

            var assistVCam = leftDist < rightDist ? leftAssistVCam : rightAssistVCam;
            assistVCam.enabled = true;

            // Suppress POV input during assist camera
            var provider = VCam.GetComponent<GameInputCameraProvider>();
            if (provider != null) provider.SuppressInput();

            var assistVcamAngles = assistVCam.transform.rotation.eulerAngles;
            var pov = VCam.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null)
                pov.m_HorizontalAxis.Value = assistVcamAngles.y;
        }

        public void ExitAssistCamera()
        {
            if (leftAssistVCam != null) leftAssistVCam.enabled = false;
            if (rightAssistVCam != null) rightAssistVCam.enabled = false;

            // Wait for brain blend to complete, then gradually restore input
            if (VCam != null)
            {
                var provider = VCam.GetComponent<GameInputCameraProvider>();
                if (provider != null) provider.BeginRestore(0.3f);
            }
        }

        [Header("Ultimate Teleport")]
        [SerializeField] private float ultimateTeleportDist = 2.5f;

        private void EnterUltimateMode()
        {
            IsUltimateMode = true;
            _ultimateCameraExited = false;

            // 0. 가장 가까운 몬스터 앞으로 텔레포트
            TeleportToClosestMonster();

            // 1. 모든 몬스터 프리즈
            foreach (var m in SimpleMonsterCtrl.AllMonsters)
                m.SetFrozen(true);

            // 2. 다른 파티원 애니메이션 정지
            var ps = SimplePlayerSystem.Instance;
            if (ps != null)
            {
                foreach (var rc in ps.RoleCtrls)
                    if (rc != this) rc.ActionCtrl.Speed = 0f;
                ps.switchEnabled = false;
            }

            // 3. 궁극기 카메라 활성화 (컷 전환)
            ForceBrainCut();
            if (ultimateVCam != null) ultimateVCam.enabled = true;
            if (VCam != null)
            {
                var provider = VCam.GetComponent<GameInputCameraProvider>();
                if (provider != null) provider.SuppressInput();
            }

            // 4. 캐릭터 렌더러를 Character 레이어(6)로 이동
            foreach (var r in _renderers)
                if (r != null) r.gameObject.layer = 6;

            // 5. 카메라 culling mask + Skybox 교체
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                _savedCullingMask = mainCam.cullingMask;
                _savedClearFlags = mainCam.clearFlags;
                _savedBgColor = mainCam.backgroundColor;
                _savedSkybox = RenderSettings.skybox;

                mainCam.cullingMask = (1 << 6) | (1 << 9) | (1 << 5);

                if (ultimateSkyboxMat != null)
                {
                    RenderSettings.skybox = ultimateSkyboxMat;
                    mainCam.clearFlags = CameraClearFlags.Skybox;
                }
                else
                {
                    mainCam.clearFlags = CameraClearFlags.SolidColor;
                    mainCam.backgroundColor = Color.black;
                }
            }

            // 7. 시네마틱 바 표시
            CinematicBars.Instance?.Show();
        }

        private void TeleportToClosestMonster()
        {
            SimpleMonsterCtrl closest = null;
            float closestDist = float.MaxValue;
            foreach (var m in SimpleMonsterCtrl.AllMonsters)
            {
                float d = Vector3.Distance(transform.position, m.transform.position);
                if (d < closestDist) { closestDist = d; closest = m; }
            }

            if (closest == null) return;

            // 몬스터를 바라보는 방향
            Vector3 dir = closest.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;
            dir.Normalize();

            // 몬스터 앞 ultimateTeleportDist 위치로 텔레포트
            Vector3 targetPos = closest.transform.position - dir * ultimateTeleportDist;
            targetPos.y = transform.position.y;

            SetPos(targetPos);
            transform.forward = dir;
        }

        private void ExitUltimateMode()
        {
            IsUltimateMode = false;

            // 1. 카메라/시간/연출 복원 (이미 퇴장한 경우 스킵)
            ExitUltimateCamera();

            // 2. 스위치 재활성화
            var ps = SimplePlayerSystem.Instance;
            if (ps != null)
                ps.switchEnabled = true;

            // 4. 캐릭터 렌더러를 Default 레이어(0)로 복원
            foreach (var r in _renderers)
                if (r != null) r.gameObject.layer = 0;

            // 5. 돔 제거
            if (_activeDome != null)
            {
                Destroy(_activeDome);
                _activeDome = null;
            }
        }

        private bool _ultimateCameraExited;

        /// <summary>궁극기 카메라 + 연출(culling/skybox/bars/시간) 복원. 중복 호출 안전.</summary>
        public void ExitUltimateCamera()
        {
            if (_ultimateCameraExited) return;
            _ultimateCameraExited = true;

            // 1. 몬스터 해동 + 파티원 Speed 복원
            foreach (var m in SimpleMonsterCtrl.AllMonsters)
                m.SetFrozen(false);
            var ps = SimplePlayerSystem.Instance;
            if (ps != null)
            {
                foreach (var rc in ps.RoleCtrls)
                    if (rc != this) rc.ActionCtrl.Speed = 1f;
            }

            // 2. 카메라 컷 전환
            ForceBrainCut();
            if (ultimateVCam != null) ultimateVCam.enabled = false;
            if (VCam != null)
            {
                var provider = VCam.GetComponent<GameInputCameraProvider>();
                if (provider != null) provider.BeginRestore(0.3f);
            }

            // 3. culling mask + skybox 복원
            var mainCam = Camera.main;
            if (mainCam != null && _savedCullingMask != 0)
            {
                mainCam.cullingMask = _savedCullingMask;
                mainCam.clearFlags = _savedClearFlags;
                mainCam.backgroundColor = _savedBgColor;
                RenderSettings.skybox = _savedSkybox;
            }

            // 4. 시네마틱 바 숨김
            CinematicBars.Instance?.Hide();

            // 5. 카메라 yaw를 캐릭터 forward에 동기화
            SyncCameraYawToFacing();
        }

        /// <summary>POV yaw를 캐릭터 forward 방향에 맞춤 (등 뒤에서 보도록)</summary>
        private void SyncCameraYawToFacing()
        {
            if (VCam == null) return;
            var pov = VCam.GetCinemachineComponent<CinemachinePOV>();
            if (pov == null) return;

            float yaw = transform.eulerAngles.y;
            pov.m_HorizontalAxis.Value = yaw;
        }

        /// <summary>Brain의 Default Blend를 Cut으로 강제 → 1프레임 후 복원</summary>
        private void ForceBrainCut()
        {
            var brain = Camera.main?.GetComponent<CinemachineBrain>();
            if (brain == null) return;

            _savedBlendStyle = brain.m_DefaultBlend.m_Style;
            _savedBlendTime = brain.m_DefaultBlend.m_Time;
            brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            brain.m_DefaultBlend.m_Time = 0f;

            RestoreBrainBlendAsync(brain).Forget();
        }

        private async UniTaskVoid RestoreBrainBlendAsync(CinemachineBrain brain)
        {
            await UniTask.DelayFrame(2);
            if (brain != null)
            {
                brain.m_DefaultBlend.m_Style = _savedBlendStyle;
                brain.m_DefaultBlend.m_Time = _savedBlendTime;
            }
        }

        public CinemachineVirtualCamera teleportBehindVCam;
        public float teleportCamYawDuration = 0.4f;
        private Tween _teleportYawTween;

        public void EnterTeleportBehindCamera(Vector3 lookDir)
        {
            if (VCam == null) return;

            var pov = VCam.GetCinemachineComponent<CinemachinePOV>();
            if (pov == null) return;

            // 이전 트윈 정리
            _teleportYawTween?.Kill();

            // POV 입력 차단
            var provider = VCam.GetComponent<GameInputCameraProvider>();
            if (provider != null) provider.SuppressInput();

            float targetYaw = Quaternion.LookRotation(lookDir).eulerAngles.y;
            float currentYaw = pov.m_HorizontalAxis.Value;

            // 최단 경로 계산 (-180 ~ 180 범위로 보정)
            float delta = Mathf.DeltaAngle(currentYaw, targetYaw);
            float endYaw = currentYaw + delta;

            // DOTween으로 POV yaw 궤도 회전
            _teleportYawTween = DOTween.To(
                () => pov.m_HorizontalAxis.Value,
                v => pov.m_HorizontalAxis.Value = v,
                endYaw,
                teleportCamYawDuration
            ).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                // yaw 보간 완료 후 입력 복원
                if (provider != null) provider.BeginRestore(0.3f);
                _teleportYawTween = null;
            });
        }

        public void ExitTeleportBehindCamera()
        {
            // 카메라 종료는 DOTween OnComplete에서 자동 처리
        }

        private Collider[] cols = new Collider[8];

        #region Debug UI

        public void DrawDebugUI(float x, float y, int slot, float w = 400f)
        {
            if (ActionCtrl == null) return;

            var action = ActionCtrl.Action;
            float lineH = 28f;
            int lines = action != null ? 4 : 1;

            var bgStyle = new GUIStyle(GUI.skin.box);
            GUI.Box(new Rect(x, y, w, lineH * lines + 12f), "", bgStyle);

            var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            var normalStyle = new GUIStyle(GUI.skin.label) { fontSize = 16 };

            bool visible = _renderers.Length > 0 && _renderers[0].enabled;
            GUI.Label(new Rect(x + 8, y + 4, w, lineH),
                $"[{slot}] {name}  {(visible ? "" : "(BG)")}", titleStyle);

            if (action == null) return;
            y += lineH;

            float time = ActionCtrl.Time;
            float duration = action.duration > 0 ? (float)action.duration : 1f;
            float progress = Mathf.Clamp01(time / duration);

            GUI.Label(new Rect(x + 8, y + 2, w, lineH),
                $"{action.name}  {time:F2}/{duration:F2}", normalStyle);
            y += lineH;

            GUI.DrawTexture(new Rect(x + 8, y + 4, w - 16f, 14f), Texture2D.grayTexture);
            GUI.DrawTexture(new Rect(x + 8, y + 4, (w - 16f) * progress, 14f), Texture2D.whiteTexture);
            y += lineH;

            string flags = (action.isLoop ? "[Loop] " : "") +
                           (IsAttacking ? "[Atk] " : "") +
                           (EnableRotation ? "[Rot] " : "") +
                           (EnableLookAtMonster ? "[LookAt]" : "");
            GUI.Label(new Rect(x + 8, y + 2, w, lineH), flags, normalStyle);
        }

        #endregion

        public bool TryTriggerPerfectDodge()
        {
            GetCharacterControllerPoints(out var p0, out var p1, out float radius);
            int size = Physics.OverlapCapsuleNonAlloc(p0, p1, radius, cols,
                LayerMask.GetMask("DodgeJudge"));
            if (size > 0)
            {
                Debug.Log("[SimpleRoleCtrl] Perfect Dodge!");
                Debug.Log($"Colliders: {string.Join(", ", cols.Take(size).Select(x => $"{x.name}(enabled: {x.enabled})"))}");
                return true;
            }
            return false;
        }

        private void GetCharacterControllerPoints(out Vector3 p0, out Vector3 p1, out float radius)
        {
            var chCtrl = CharacterController;
            var offset = new Vector3(0f, chCtrl.height * 0.5f - chCtrl.radius, 0f);
            var p0Local = chCtrl.center - offset;
            var p1Local = chCtrl.center + offset;
            p0 = transform.TransformPoint(p0Local);
            p1 = transform.TransformPoint(p1Local);
            radius = chCtrl.radius;
        }
    }
}
