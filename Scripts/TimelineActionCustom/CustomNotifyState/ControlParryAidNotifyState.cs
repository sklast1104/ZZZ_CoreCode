using System.ComponentModel;

namespace JM.TimelineAction
{
    [DisplayName("패링 어시스트 제어")]
    public class ControlParryAidNotifyState : ActionNotifyState
    {
        public override void NotifyBegin(ActionCtrl actionCtrl)
        {
            if (actionCtrl.TryGetComponent<RoleCtrl>(out var roleCtrl))
            {
                NetFn.Send(new MsgRoleSetParry()
                {
                    RoleId = roleCtrl.Role.Id,
                    Parrying = true
                });
            }
            else if (actionCtrl.TryGetComponent<SimpleRoleCtrl>(out var simpleRoleCtrl))
            {
                simpleRoleCtrl.IsParrying = true;
            }
        }

        public override void NotifyEnd(ActionCtrl actionCtrl)
        {
            if (actionCtrl.TryGetComponent<RoleCtrl>(out var roleCtrl))
            {
                NetFn.Send(new MsgRoleSetParry()
                {
                    RoleId = roleCtrl.Role.Id,
                    Parrying = false
                });
            }
            else if (actionCtrl.TryGetComponent<SimpleRoleCtrl>(out var simpleRoleCtrl))
            {
                simpleRoleCtrl.IsParrying = false;
            }
        }
    }
}