using System.ComponentModel;
using UnityEngine.Timeline;

namespace JM.TimelineAction
{
    [TrackClipType(typeof(BoxNotifyState)), DisplayName("히트박스 트랙")]
    public class BoxTrack : TrackAsset
    {
        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);
            clip.duration = 0.25f;
            clip.displayName = "히트박스";
        }
    }
}