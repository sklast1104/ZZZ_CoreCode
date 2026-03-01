using System.ComponentModel;
using UnityEngine;

namespace JM.TimelineAction
{
    [DisplayName("액션 파라미터/전환 오버라이드")]
    public class OverrideActionParamsAndTransitionNotify : ActionNotify
    {
        [TimelineActionName]
        public string actionName;

        public override void Notify(ActionCtrl actionCtrl)
        {
            if (actionCtrl.TryGetAction(actionName, out var action))
            {
                actionCtrl.OverrideAction = action;
                actionCtrl.OnSetActionArgs?.Invoke(action.actionArgs);
            }
            else
            {
                Debug.LogWarning($"액션을 찾을 수 없음, 액션명: {actionName}");
            }
        }
    }
}