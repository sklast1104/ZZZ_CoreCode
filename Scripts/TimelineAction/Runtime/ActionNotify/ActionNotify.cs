using UnityEngine.Timeline;

namespace JM.TimelineAction
{
    public abstract class ActionNotify : Marker
    {
        public virtual void Notify(ActionCtrl actionCtrl)
        {
        }
    }
}