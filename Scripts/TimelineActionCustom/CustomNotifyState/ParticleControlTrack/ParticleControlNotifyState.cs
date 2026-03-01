using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

namespace JM.TimelineAction
{
    [DisplayName("파티클 제어")]
    public class ParticleControlNotifyState : ActionNotifyState
    {
        [NonSerialized] public GameObject owner;
        [NonSerialized] public GameObject previewGo;
        public GameObject prefab;
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            this.owner = owner;
            return ParticleControlPlayable.Create(graph, owner, this);
        }

        public override void NotifyBegin(ActionCtrl actionCtrl)
        {
            var transform = actionCtrl.transform;
            var particle = ParticleMgr.Instance.PlayAsChild(prefab, transform);
            particle.transform.localPosition = position;
            particle.transform.localRotation = rotation;
            particle.transform.localScale = scale;
            ParticleControlPlayable.SetScalingModeHierarchy(particle);
        }
    }
}