using System.ComponentModel;
using UnityEngine.Timeline;

namespace JM.TimelineAction
{
    [TrackClipType(typeof(ActionNotifyState)), DisplayName("진입 트랙")]
    public class EnterTrack : ActionNotifyStateTrack
    {
    }
}