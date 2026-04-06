using System.ComponentModel;
using UnityEngine.Timeline;

namespace JM.TimelineAction
{
    [TrackClipType(typeof(RandomAudioNotifyState)),
     DisplayName("랜덤 오디오 트랙")]
    public class RandomAudioTrack : TrackAsset
    {
        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);
            clip.duration = 1f;
            clip.displayName = "랜덤 오디오";
        }
    }
}