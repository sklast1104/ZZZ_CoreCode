using System.ComponentModel;

namespace JM.TimelineAction
{
    [DisplayName("어시스트 카메라 제어")]
    public class AssistCameraNotifyState : ActionNotifyState
    {
        public override void NotifyBegin(ActionCtrl actionCtrl)
        {
            var roleCtrl = actionCtrl.GetComponent<RoleCtrl>();
            if (roleCtrl) { roleCtrl.EnterAssistCamera(); return; }

            var simple = actionCtrl.GetComponent<SimpleRoleCtrl>();
            if (simple) simple.EnterAssistCamera();
        }

        public override void NotifyEnd(ActionCtrl actionCtrl)
        {
            var roleCtrl = actionCtrl.GetComponent<RoleCtrl>();
            if (roleCtrl) { roleCtrl.ExitAssistCamera(); return; }

            var simple = actionCtrl.GetComponent<SimpleRoleCtrl>();
            if (simple) simple.ExitAssistCamera();
        }
    }
}