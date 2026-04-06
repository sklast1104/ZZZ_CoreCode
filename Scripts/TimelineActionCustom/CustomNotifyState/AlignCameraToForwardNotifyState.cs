using System.ComponentModel;
using UnityEngine;

namespace JM.TimelineAction
{
    [DisplayName("카메라 캐릭터 정면 정렬")]
    public class AlignCameraToForwardNotifyState : ActionNotifyState
    {
        public override void NotifyBegin(ActionCtrl actionCtrl)
        {
            base.NotifyBegin(actionCtrl);

            if (!actionCtrl.TryGetComponent(out SimpleRoleCtrl roleCtrl)) return;

            Vector3 fwd = actionCtrl.transform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.001f) return;

            roleCtrl.EnterTeleportBehindCamera(fwd);
        }

        public override void NotifyEnd(ActionCtrl actionCtrl)
        {
            // 카메라 복원은 DOTween OnComplete에서 자동 처리
        }
    }
}
