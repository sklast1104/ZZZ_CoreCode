using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace JM.TimelineAction
{
    [CreateAssetMenu(fileName = "TimelineActionSO", menuName = "TimelineAction/ActionSO")]
    public class ActionSO : TimelineAsset
    {
        // 给动作Id，判断动作是否可执行用
        public int actionId;

        public string actionType;

        public bool isLoop;

        [FormerlySerializedAs("actionParams")]
        public ActionArgs actionArgs;

        [FormerlySerializedAs("finishCancelInfo")]
        public TransitionInfo finishTransition;

        [FormerlySerializedAs("inheritTransitionActionName")]
        [TimelineActionName]
        public string inheritActionTransition;
        public List<CommandTransitionInfo> commandTransitions;
        public List<SignalTransitionInfo> signalTransitions;
    }
}