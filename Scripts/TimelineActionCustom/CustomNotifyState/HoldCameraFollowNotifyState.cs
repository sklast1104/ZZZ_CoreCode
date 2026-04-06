using System.ComponentModel;
using UnityEngine;

namespace JM.TimelineAction
{
    [DisplayName("카메라 추적 고정")]
    public class HoldCameraFollowNotifyState : ActionNotifyState
    {
        private Transform vcamFollow;
        private Vector3 worldPos;
        private Vector3 localPos;

        public override void NotifyBegin(ActionCtrl actionCtrl)
        {
            var ch = actionCtrl.GetComponent<RoleCtrl>();
            if (ch != null)
            {
                vcamFollow = ch.vcamFollow;
            }
            else
            {
                var simple = actionCtrl.GetComponent<SimpleRoleCtrl>();
                if (simple != null) vcamFollow = simple.vcamFollow;
            }

            if (vcamFollow != null)
            {
                worldPos = vcamFollow.position;
                localPos = vcamFollow.localPosition;
            }
        }

        public override void NotifyTick(ActionCtrl actionCtrl, float time)
        {
            if (vcamFollow != null)
            {
                vcamFollow.position = worldPos;
            }
        }

        public override void NotifyEnd(ActionCtrl actionCtrl)
        {
            if (vcamFollow != null)
            {
                vcamFollow.localPosition = localPos;
            }
        }
    }
}