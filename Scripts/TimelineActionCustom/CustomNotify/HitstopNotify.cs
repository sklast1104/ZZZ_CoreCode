using System.ComponentModel;

namespace JM.TimelineAction
{
    [DisplayName("히트스톱")]
    public class HitstopNotify : ActionNotify
    {
        [TimeField(60)]
        public float duration = 0.05f;
        public float animationSpeed;

        public override void Notify(ActionCtrl actionCtrl)
        {
            // var roleCtrl = actionCtrl.GetComponent<RoleCtrl>();
            // if (!roleCtrl) return;
            // roleCtrl.TriggerHitstopIfHitMonster(duration, animationSpeed);
        }
    }
}