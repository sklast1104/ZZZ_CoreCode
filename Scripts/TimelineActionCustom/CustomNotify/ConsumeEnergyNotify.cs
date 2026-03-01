using System.ComponentModel;

namespace JM.TimelineAction
{
    [DisplayName("에너지 소모")]
    public class ConsumeEnergyNotify : ActionNotify
    {
        public float cost;

        public override void Notify(ActionCtrl actionCtrl)
        {
            base.Notify(actionCtrl);
            if (actionCtrl.TryGetComponent<RoleCtrl>(out var roleCtrl))
            {
                roleCtrl.ConsumeEnergy(cost);
            }
        }
    }
}