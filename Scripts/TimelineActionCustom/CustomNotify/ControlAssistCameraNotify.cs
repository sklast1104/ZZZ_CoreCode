using System.ComponentModel;

namespace JM.TimelineAction
{
    [DisplayName("패링 어시스트 카메라 제어")]
    public class ControlAssistCameraNotify : ActionNotify
    {
        public bool enter;

        public override void Notify(ActionCtrl actionCtrl)
        {
            var roleCtrl = actionCtrl.GetComponent<RoleCtrl>();
            if (roleCtrl)
            {
                if (enter) roleCtrl.EnterAssistCamera();
                else roleCtrl.ExitAssistCamera();
                return;
            }

            var simple = actionCtrl.GetComponent<SimpleRoleCtrl>();
            if (simple)
            {
                if (enter) simple.EnterAssistCamera();
                else simple.ExitAssistCamera();
            }
        }
    }
}