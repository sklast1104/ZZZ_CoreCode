using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JM.TimelineAction;
using UnityEngine;

namespace JM
{
    public class SimpleMonsterCtrl : MonoBehaviour
    {
        public static readonly List<SimpleMonsterCtrl> AllMonsters = new();
        public static readonly List<SimpleMonsterCtrl> DodgeDetectMonsters = new();

        public static event global::System.Action<SimpleMonsterCtrl, float> OnAnyTakeDamage;

        [SerializeField] private float maxHp = 100f;

        [Header("닷지 판정 콜라이더 (DodgeJudge 레이어)")]
        [SerializeField] private SphereCollider dodgeSphereCollider;
        [SerializeField] private BoxCollider dodgeBoxCollider;

        [Serializable]
        public struct HitStrengthMapping
        {
            public EHitStrength hitStrength;
            [Tooltip("액션 접두사 (예: hit_body, hit_heavy)")]
            public string actionPrefix;
        }

        [Header("슈퍼아머")]
        [SerializeField] private bool enableSuperArmor = true;
        [SerializeField] private int staggerThreshold = 4;
        [SerializeField] private float staggerResetTime = 3f;
        [SerializeField] private float superArmorHitstopDuration = 0.25f;

        [Header("공격 AI")]
        [SerializeField] private bool disableAttackAI = false;
        [SerializeField] private float attackRange = 8f;
        [SerializeField] private float attackInterval = 3f;

        [Header("히트 리액션")]
        [SerializeField] private HitStrengthMapping[] hitMappings = new[]
        {
            new HitStrengthMapping { hitStrength = EHitStrength.Low, actionPrefix = "hit_body" },
            new HitStrengthMapping { hitStrength = EHitStrength.High, actionPrefix = "hit_heavy" },
        };

        public float CurrentHp { get; private set; }
        public float MaxHp => maxHp;
        public SimpleRoleCtrl ParryingRole { get; set; }
        private ActionCtrl ActionCtrl { get; set; }
        private CharacterController characterController;
        private Animator animator;

        // 히트박스 추적 (몬스터→캐릭터 공격)
        private BoxNotifyState activeHitBox;
        private readonly HashSet<SimpleRoleCtrl> hitRoles = new();

        // 공격 AI 상태
        private float attackTimer;
        private bool isAttacking;

        // 슈퍼아머 상태
        private int _staggerCount;
        private float _lastHitTime;
        private bool _superArmor;
        private float _saHitstopRemaining; // 슈퍼아머 누적 히트스톱 잔여시간

        // 히트스톱 토큰 (중복 호출 시 이전 것 무효화)
        private int _hitstopToken;

        // 프리즈 상태 (궁극기 연출)
        private bool _isFrozen;

        // 넉백 상태
        private Vector3 knockbackVelocity;
        private float knockbackTimeRemaining;

        private static readonly (string name, Vector3 dir)[] HitDirections =
        {
            ("Front", Vector3.forward),
            ("Back",  Vector3.back),
            ("Left",  Vector3.left),
            ("Right", Vector3.right),
        };

        private void OnEnable() { AllMonsters.Add(this); }
        private void OnDisable()
        {
            AllMonsters.Remove(this);
            DodgeDetectMonsters.Remove(this);
        }

        private void Awake()
        {
            CurrentHp = maxHp;

            ActionCtrl = GetComponent<ActionCtrl>();
            ActionCtrl.IsActionExecutable = _ => true;
            ActionCtrl.OnExecuteAction = (action, _) =>
            {
                bool wasAttacking = isAttacking;
                isAttacking = action.name.Contains("Attack");

                // 슈퍼아머 해제: Attack이 아닌 액션 진입 시 리셋
                if (_superArmor && !isAttacking)
                {
                    _superArmor = false;
                    _staggerCount = 0;
                    _saHitstopRemaining = 0f;
                    ActionCtrl.Speed = 1f;
                }
            };
            ActionCtrl.OnSetActionArgs = _ => { };

            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            if (dodgeSphereCollider != null) dodgeSphereCollider.enabled = false;
            if (dodgeBoxCollider != null) dodgeBoxCollider.enabled = false;
        }

        private void Start()
        {
            ActionCtrl.PlayAction("Idle");
            attackTimer = 2f; // 첫 공격까지 대기
        }

        private void OnAnimatorMove()
        {
            // 히트 애니메이션 루트모션은 적용하지 않음 (넉백 별도 구현 예정)
        }

        // 서버 MonsterCtrl.UpdateCollisionDetection() 재현
        private const float RoleRadius = 0.55f;
        private const float MonsterRadius = 0.4f;
        private static readonly Collider[] overlapBuf = new Collider[4];

        public void SetFrozen(bool frozen)
        {
            _isFrozen = frozen;
            ActionCtrl.Speed = frozen ? 0f : 1f;
        }

        private void LateUpdate()
        {
            if (_isFrozen) return;

            UpdateSuperArmorHitstop();
            UpdateKnockback();
            UpdateCollisionSeparation();
            UpdateHitDetection();
            UpdateAttackAI();
        }

        private void UpdateSuperArmorHitstop()
        {
            if (_saHitstopRemaining <= 0f) return;

            _saHitstopRemaining -= Time.deltaTime;
            if (_saHitstopRemaining <= 0f)
            {
                _saHitstopRemaining = 0f;
                ActionCtrl.Speed = 1f;
            }
            else
            {
                ActionCtrl.Speed = 0f;
            }
        }

        private void UpdateHitDetection()
        {
            if (activeHitBox != null)
                SimpleAttackProcess.MonsterAttack(this, activeHitBox, hitRoles);
        }

        private void UpdateKnockback()
        {
            if (knockbackTimeRemaining <= 0f) return;
            if (characterController == null || !characterController.enabled) return;

            float dt = Time.deltaTime;
            knockbackTimeRemaining -= dt;

            if (knockbackTimeRemaining <= 0f)
            {
                knockbackTimeRemaining = 0f;
                knockbackVelocity = Vector3.zero;
                return;
            }

            characterController.Move(knockbackVelocity * dt);

            // ease-out: 속도가 남은 시간 비율로 감쇠
            float decay = knockbackTimeRemaining / (knockbackTimeRemaining + dt);
            knockbackVelocity *= decay;
        }

        private void UpdateCollisionSeparation()
        {
            if (characterController == null || !characterController.enabled) return;

            int count = Physics.OverlapSphereNonAlloc(
                transform.position, RoleRadius + MonsterRadius,
                overlapBuf, LayerMask.GetMask("Character"));

            for (int i = 0; i < count; i++)
            {
                // 해당 캐릭터가 밀쳐내기 비활성화 중이면 스킵
                var roleCtrl = overlapBuf[i].GetComponent<SimpleRoleCtrl>();
                if (roleCtrl != null && roleCtrl.DisableMonsterPush) continue;

                var rolePos = overlapBuf[i].transform.position;
                var monsterPos = transform.position;

                var dir = monsterPos - rolePos;
                dir.y = 0f;
                float dist = dir.magnitude;

                float minDist = RoleRadius + MonsterRadius;
                if (dist >= minDist || dist < 0.001f) continue;

                var pushDir = dir.normalized;
                var delta = pushDir * (minDist - dist);
                characterController.Move(delta);
            }
        }

        public void TakeDamage(float damage, Vector3 attackerPos, EHitStrength hitStrength,
            float knockbackDist, float knockbackDuration = 0f)
        {
            CurrentHp = Mathf.Max(0f, CurrentHp - damage);
            OnAnyTakeDamage?.Invoke(this, damage);
            Debug.Log($"[Monster] HP: {CurrentHp}/{maxHp}");

            if (CurrentHp <= 0f)
            {
                Debug.Log("[Monster] Destroyed!");
                Destroy(gameObject);
                return;
            }

            // 슈퍼아머 카운터 관리
            if (Time.time - _lastHitTime > staggerResetTime)
                _staggerCount = 0;
            _lastHitTime = Time.time;
            _staggerCount++;

            // 슈퍼아머 중: 경직 스킵, 넉백+히트스톱은 적용
            if (_superArmor)
            {
                _saHitstopRemaining += superArmorHitstopDuration;
                ActionCtrl.Speed = 0f;

                // 넉백은 슈퍼아머 중에도 적용
                ApplyKnockback(attackerPos, knockbackDist, knockbackDuration);
                return;
            }

            // 임계값 도달 → 슈퍼아머 발동 + 반격
            if (enableSuperArmor && _staggerCount >= staggerThreshold)
            {
                _superArmor = true;
                Debug.Log("[Monster] 슈퍼아머 발동! → Attack_01");

                // 공격자를 향해 회전
                Vector3 faceDir = attackerPos - transform.position;
                faceDir.y = 0f;
                if (faceDir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(faceDir);

                ActionCtrl.PlayAction("Attack_01");
                return;
            }

            // 1. 방향 계산 (회전 전에)
            string direction = ChooseHitDirection(attackerPos);

            // 2. 넉백
            ApplyKnockback(attackerPos, knockbackDist, knockbackDuration);

            // 3. 공격자를 향해 회전
            Vector3 faceDir2 = attackerPos - transform.position;
            faceDir2.y = 0f;
            if (faceDir2.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(faceDir2);

            // 4. 히트 애니메이션 재생
            string prefix = GetHitPrefix(hitStrength);
            if (prefix != null)
            {
                string actionName = $"{prefix}_{direction}";
                ActionCtrl.PlayAction(actionName, 0.05f);
            }
        }

        public async UniTaskVoid TriggerHitstop(float duration, float speed)
        {
            if (duration <= 0f) return;

            // 슈퍼아머 중에는 외부 히트스톱 무시 (누적 타이머로 처리)
            if (_superArmor) return;

            int token = ++_hitstopToken;
            ActionCtrl.Speed = speed;
            await UniTask.WaitForSeconds(duration);
            if (this == null) return;
            if (_hitstopToken != token) return;
            ActionCtrl.Speed = _isFrozen ? 0f : 1f;
        }

        public void BoxBegin(BoxNotifyState box)
        {
            switch (box.boxType)
            {
                case EBoxType.HitBox:
                    BeginHit(box);
                    break;
                case EBoxType.DodgeBox:
                    BeginDodgeDetect(box);
                    break;
            }
        }

        public void BoxEnd(BoxNotifyState box)
        {
            if (box.boxType == EBoxType.DodgeBox)
            {
                EndDodgeDetect(box);
            }
            else if (box.boxType == EBoxType.HitBox)
            {
                if (activeHitBox == box)
                {
                    activeHitBox = null;
                    hitRoles.Clear();
                }
            }
        }

        private void UpdateAttackAI()
        {
            if (disableAttackAI) return;
            if (isAttacking) return;

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f) return;

            // 가장 가까운 캐릭터 탐색
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, attackRange,
                overlapBuf, LayerMask.GetMask("Character"));

            Transform target = null;
            float closestDist = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                float d = Vector3.Distance(transform.position, overlapBuf[i].transform.position);
                if (d < closestDist) { closestDist = d; target = overlapBuf[i].transform; }
            }

            if (target == null)
            {
                attackTimer = 0.5f; // 재탐색 대기
                return;
            }

            // 타겟 방향으로 회전
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);

            ActionCtrl.PlayAction("Attack_01");
            attackTimer = attackInterval;
        }

        private void BeginDodgeDetect(BoxNotifyState box)
        {
            switch (box.boxShape)
            {
                case EBoxShape.Sphere:
                    if (dodgeSphereCollider != null)
                    {
                        dodgeSphereCollider.center = box.center;
                        dodgeSphereCollider.radius = box.radius;
                        dodgeSphereCollider.enabled = true;
                    }
                    break;
                case EBoxShape.Box:
                    if (dodgeBoxCollider != null)
                    {
                        dodgeBoxCollider.center = box.center;
                        dodgeBoxCollider.size = box.size;
                        dodgeBoxCollider.enabled = true;
                    }
                    break;
            }
            DodgeDetectMonsters.Add(this);
        }

        private void EndDodgeDetect(BoxNotifyState box)
        {
            switch (box.boxShape)
            {
                case EBoxShape.Sphere:
                    if (dodgeSphereCollider != null)
                        dodgeSphereCollider.enabled = false;
                    break;
                case EBoxShape.Box:
                    if (dodgeBoxCollider != null)
                        dodgeBoxCollider.enabled = false;
                    break;
            }
            DodgeDetectMonsters.Remove(this);
            ParryingRole = null;
        }

        private void BeginHit(BoxNotifyState box)
        {
            if (ParryingRole != null)
            {
                Debug.Log("[Monster] 패리 발동!");
                ParryingRole.EnterAttackParryAid().Forget();
                EnterParried().Forget();
                ParryingRole = null;
                return;
            }

            activeHitBox = box;
            hitRoles.Clear();
            SimpleAttackProcess.MonsterAttack(this, box, hitRoles);
        }

        public async UniTaskVoid EnterParried()
        {
            const float duration = 0.5f;
            ActionCtrl.Speed = 0f;
            await UniTask.WaitForSeconds(duration);
            if (this == null) return;
            ActionCtrl.Speed = 1f;
        }

        public static SimpleMonsterCtrl ClosestDodgeDetectMonster(Vector3 worldPos, out float dist)
        {
            dist = float.MaxValue;
            SimpleMonsterCtrl closest = null;
            foreach (var m in DodgeDetectMonsters)
            {
                float d = Vector3.Distance(worldPos, m.transform.position);
                if (d < dist) { dist = d; closest = m; }
            }
            return closest;
        }

        private void ApplyKnockback(Vector3 attackerPos, float knockbackDist, float knockbackDuration)
        {
            float absDist = Mathf.Abs(knockbackDist);
            if (absDist <= 0.001f || characterController == null || !characterController.enabled) return;

            Vector3 moveDir = knockbackDist > 0f
                ? (transform.position - attackerPos)
                : (attackerPos - transform.position);
            moveDir.y = 0f;
            if (moveDir.sqrMagnitude <= 0.001f) return;
            moveDir = moveDir.normalized;

            if (knockbackDuration <= 0f)
            {
                characterController.Move(moveDir * absDist);
            }
            else
            {
                float initialSpeed = 2f * absDist / knockbackDuration;
                knockbackVelocity = moveDir * initialSpeed;
                knockbackTimeRemaining = knockbackDuration;
            }
        }

        private string ChooseHitDirection(Vector3 attackerPos)
        {
            Vector3 toAttacker = attackerPos - transform.position;
            toAttacker.y = 0f;
            Vector3 localDir = transform.InverseTransformDirection(toAttacker.normalized);

            float maxDot = float.MinValue;
            string best = "Front";
            foreach (var (name, dir) in HitDirections)
            {
                float dot = Vector3.Dot(localDir, dir);
                if (dot > maxDot) { maxDot = dot; best = name; }
            }
            return best;
        }

        private string GetHitPrefix(EHitStrength strength)
        {
            foreach (var m in hitMappings)
                if (m.hitStrength == strength) return m.actionPrefix;
            return hitMappings.Length > 0 ? hitMappings[0].actionPrefix : null;
        }
    }
}
