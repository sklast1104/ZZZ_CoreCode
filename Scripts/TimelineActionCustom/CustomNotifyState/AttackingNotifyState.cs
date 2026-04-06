
using System.ComponentModel;

namespace JM.TimelineAction
{
    [DisplayName("공격 중 상태")]
    public class AttackingNotifyState : ActionNotifyState
    {
        public override void NotifyBegin(ActionCtrl actionCtrl)
        {
            base.NotifyBegin(actionCtrl);
            if (actionCtrl.TryGetComponent(out RoleCtrl roleCtrl))
            {
                roleCtrl.IsAttacking = true;
            }
            else if (actionCtrl.TryGetComponent(out SimpleRoleCtrl simpleRoleCtrl))
            {
                simpleRoleCtrl.IsAttacking = true;
            }
        }

        public override void NotifyEnd(ActionCtrl actionCtrl)
        {
            base.NotifyEnd(actionCtrl);
            if (actionCtrl.TryGetComponent(out RoleCtrl roleCtrl))
            {
                roleCtrl.IsAttacking = false;
                if (roleCtrl != PlayerSystem.Instance.FrontRoleCtrl)
                {
                    actionCtrl.PlayAction(ActionName.Background);
                }
            }
            else if (actionCtrl.TryGetComponent(out SimpleRoleCtrl simpleRoleCtrl))
            {
                simpleRoleCtrl.IsAttacking = false;
            }
        }
    }
}