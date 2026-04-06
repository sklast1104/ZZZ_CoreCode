using System.ComponentModel;
using System.Reflection;
using UnityEngine.Timeline;

namespace JM.TimelineAction
{
    [TrackClipType(typeof(ActionNotifyState)), DisplayName("노티파이 상태 트랙")]
    public class ActionNotifyStateTrack : TrackAsset
    {
        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);

            var state = (ActionNotifyState)clip.asset;
            var type = state.GetType();
            var attr = type.GetCustomAttribute<NotifyStateDurationAttribute>();
            if (attr != null)
            {
                clip.duration = attr.Duration;
            }
            else
            {
                clip.duration = 0.5;
            }
            var nameAttr = type.GetCustomAttribute<DisplayNameAttribute>();
            if (nameAttr != null)
            {
                clip.displayName = nameAttr.DisplayName;
            }
        }
    }
}