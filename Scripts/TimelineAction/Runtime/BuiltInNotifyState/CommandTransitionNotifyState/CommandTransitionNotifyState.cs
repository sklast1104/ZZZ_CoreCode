using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace JM.TimelineAction
{
    [DisplayName("커맨드 전환")]
    public class CommandTransitionNotifyState : ActionNotifyState
    {
        [TimeField(60)]
        public float inputBufferDuration = 0.1f;

        [FormerlySerializedAs("cancelInfo")] public CommandTransitionInfo commandTransition;

        public override void NotifyBegin(ActionCtrl actionCtrl)
        {
            // 입력 버퍼에 해당 입력이 있으면 전환 처리
            double time = Time.timeAsDouble;
            for (int i = actionCtrl.InputBuffer.Count - 1; i >= 0; i--)
            {
                var item = actionCtrl.InputBuffer[i];
                if (!item.used &&
                    item.command == commandTransition.command &&
                    item.phase == commandTransition.phase &&
                    item.time >= time - inputBufferDuration &&
                    commandTransition.CheckModifier(actionCtrl) &&
                    actionCtrl.IsActionExecutableInternal(commandTransition.actionName))
                {
                    item.used = true;
                    actionCtrl.InputBuffer[i] = item;
                    actionCtrl.PlayAction(commandTransition.actionName, commandTransition.fadeDuration);
                    return;
                }
            }
        }
    }
}