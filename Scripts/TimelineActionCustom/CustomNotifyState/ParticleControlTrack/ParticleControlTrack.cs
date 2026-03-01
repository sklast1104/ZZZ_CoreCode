using System.ComponentModel;
using UnityEngine.Timeline;

namespace JM.TimelineAction
{
    [TrackClipType(typeof(ParticleControlNotifyState)), DisplayName("파티클 제어 트랙")]
    public class ParticleControlTrack : TrackAsset
    {
        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);
            clip.duration = 1f;
        }
    }
}