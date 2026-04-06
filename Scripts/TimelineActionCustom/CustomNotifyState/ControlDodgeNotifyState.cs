using System.ComponentModel;

namespace JM.TimelineAction
{
    [DisplayName("회피 제어")]
    public class ControlDodgeNotifyState : ActionNotifyState
    {
        public override void NotifyBegin(ActionCtrl actionCtrl)
        {
            base.NotifyBegin(actionCtrl);
            // 서버에 회피 정보 전송뿐만 아니라, 진입 시 퍼펙트 회피 판정도 수행

            if (actionCtrl.TryGetComponent<RoleCtrl>(out var roleCtrl))
            {
                roleCtrl.TryTriggerPerfectDodge();
                NetFn.Send(new MsgRoleSetDodge
                {
                    RoleId = roleCtrl.Role.Id,
                    Dodging = true
                });
            }
            else if (actionCtrl.TryGetComponent<SimpleRoleCtrl>(out var simpleRoleCtrl))
            {
                simpleRoleCtrl.TryTriggerPerfectDodge();
            }
        }

        public override void NotifyEnd(ActionCtrl actionCtrl)
        {
            base.NotifyEnd(actionCtrl);
            if (actionCtrl.TryGetComponent<RoleCtrl>(out var roleCtrl))
            {
                NetFn.Send(new MsgRoleSetDodge
                {
                    RoleId = roleCtrl.Role.Id,
                    Dodging = false
                });
            }
        }
    }
}