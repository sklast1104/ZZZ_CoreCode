using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace JM.TimelineAction
{
    [DisplayName("히트박스")]
    public class BoxNotifyState : ActionNotifyState
    {
        [NonSerialized] public GameObject owner;

        public EBoxType boxType = EBoxType.HitBox;
        public EBoxShape boxShape = EBoxShape.Sphere;
        public Vector3 center = new(0, 1, 1.5f);
        public float radius = 1f;
        public Vector3 size = new(2, 2, 2);
        public EHitStrength hitStrength;
        public int hitId;
        public GameObject particlePrefab;
        [FormerlySerializedAs("setRot")] public bool setParticleRot;
        public float rotValue;
        public float rotMaxValue;
        public float hitGatherDist;
        [TimeField(60)]
        public float knockbackDuration;
        public AudioClip hitAudio;

        // 히트스톱
        [TimeField(60)]
        public float hitstopDuration = 0.05f;
        public float hitstopSpeed = 0f;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            this.owner = owner;
            var playable = ScriptPlayable<BoxPlayable>.Create(graph);
            playable.GetBehaviour().asset = this;
            return playable;
        }

        public override void NotifyBegin(ActionCtrl actionCtrl)
        {
            if (actionCtrl.TryGetComponent<MonsterCtrl>(out var monsterCtrl))
            {
                monsterCtrl.BoxBegin(this);
            }
            else if (actionCtrl.TryGetComponent<RoleCtrl>(out var roleCtrl))
            {
                roleCtrl.BoxBegin(this);
            }
            else if (actionCtrl.TryGetComponent<SimpleRoleCtrl>(out var simpleRoleCtrl))
            {
                simpleRoleCtrl.BoxBegin(this);
            }
            else if (actionCtrl.TryGetComponent<SimpleMonsterCtrl>(out var simpleMonsterCtrl))
            {
                simpleMonsterCtrl.BoxBegin(this);
            }
        }

        public override void NotifyEnd(ActionCtrl actionCtrl)
        {
            if (actionCtrl.TryGetComponent<MonsterCtrl>(out var monsterCtrl))
            {
                monsterCtrl.BoxEnd(this);
            }
            else if (actionCtrl.TryGetComponent<SimpleRoleCtrl>(out var simpleRoleCtrl))
            {
                simpleRoleCtrl.BoxEnd(this);
            }
            else if (actionCtrl.TryGetComponent<SimpleMonsterCtrl>(out var simpleMonsterCtrl))
            {
                simpleMonsterCtrl.BoxEnd(this);
            }
        }
    }
}