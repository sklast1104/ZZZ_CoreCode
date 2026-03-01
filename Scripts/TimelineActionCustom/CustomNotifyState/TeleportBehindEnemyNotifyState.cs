
using System.ComponentModel;
using UnityEngine;

namespace JM.TimelineAction
{
    [DisplayName("적 뒤로 텔레포트")]
    public class TeleportBehindEnemyNotifyState : ActionNotifyState
    {
        public float behindDistance = 1.5f;

        public override void NotifyBegin(ActionCtrl actionCtrl)
        {
            base.NotifyBegin(actionCtrl);

            if (!actionCtrl.TryGetComponent(out SimpleRoleCtrl roleCtrl)) return;

            // 가장 가까운 몬스터 찾기
            SimpleMonsterCtrl closest = null;
            float minDist = float.MaxValue;
            foreach (var m in SimpleMonsterCtrl.AllMonsters)
            {
                float d = Vector3.Distance(actionCtrl.transform.position, m.transform.position);
                if (d < minDist) { minDist = d; closest = m; }
            }

            if (closest == null) return;

            Vector3 monsterPos = closest.transform.position;
            Vector3 monsterFwd = closest.transform.forward;

            // 몬스터 뒤로 텔레포트 (몬스터 forward 반대 방향)
            Vector3 behindPos = monsterPos - monsterFwd * behindDistance;
            roleCtrl.SetPos(behindPos);

            // 몬스터를 바라보도록 회전
            Vector3 lookDir = monsterPos - behindPos;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude < 0.001f) return;

            actionCtrl.transform.forward = lookDir.normalized;

            // 카메라 전환 (Assist Camera 패턴 — Brain 블렌드로 부드러운 전환)
            roleCtrl.EnterTeleportBehindCamera(lookDir);
        }

        public override void NotifyEnd(ActionCtrl actionCtrl)
        {
            // EnterTrack은 NotifyBegin 직후 같은 프레임에 NotifyEnd 호출
            // 카메라 종료는 SimpleRoleCtrl 코루틴이 holdDuration 후 자동 처리
        }
    }
}
