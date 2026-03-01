using System.ComponentModel;

namespace JM.TimelineAction
{
    [DisplayName("공격 팁 알림")]
    public class AttackTipNotify : ActionNotify
    {
        public bool canParry;

        public override void Notify(ActionCtrl actionCtrl)
        {
            if (actionCtrl.TryGetComponent<MonsterCtrl>(out var monsterCtrl))
            {
                MonsterSystem.Instance.DoAttackTip(monsterCtrl, canParry);
            }
        }
    }
}