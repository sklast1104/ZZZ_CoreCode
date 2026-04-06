using System.ComponentModel;
using UnityEngine.Timeline;

namespace JM.TimelineAction
{
    [TrackClipType(typeof(CommandTransitionNotifyState)),
     DisplayName("커맨드 전환 트랙")]
    public class CommandTransitionTrack : TrackAsset
    {
        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);
            clip.duration = 0.5f;
            clip.displayName = "전환";
        }
    }
}