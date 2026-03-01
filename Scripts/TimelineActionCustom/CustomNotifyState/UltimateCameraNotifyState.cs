using System.ComponentModel;

namespace JM.TimelineAction
{
    [DisplayName("궁극기 카메라 제어")]
    public class UltimateCameraNotifyState : ActionNotifyState
    {
        public override void NotifyBegin(ActionCtrl actionCtrl) { }

        public override void NotifyEnd(ActionCtrl actionCtrl)
        {
            if (actionCtrl.TryGetComponent(out SimpleRoleCtrl roleCtrl))
                roleCtrl.ExitUltimateCamera();
        }
    }
}
