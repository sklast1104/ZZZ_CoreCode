using UnityEngine;
using UnityEngine.Playables;

namespace JM.TimelineAction
{
    public class ParticleControlPlayable : PlayableBehaviour
    {
        private GameObject go;
        private ParticleSystem particle;
        private ParticleControlNotifyState notifyState;

        public static ScriptPlayable<ParticleControlPlayable> Create(
            PlayableGraph graph, GameObject owner,
            ParticleControlNotifyState notifyState)
        {
            var playable = ScriptPlayable<ParticleControlPlayable>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.Init(owner, notifyState);
            return playable;
        }

        private void Init(GameObject owner, ParticleControlNotifyState notifyState)
        {
            this.notifyState = notifyState;
#if UNITY_EDITOR
            if (notifyState.prefab == null) return;

            go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(notifyState.prefab, owner.transform);
            notifyState.previewGo = go;

            go.transform.localPosition = notifyState.position;
            go.transform.localRotation = notifyState.rotation;
            go.transform.localScale = notifyState.scale;
            SetHideFlagsAll(go, HideFlags.DontSave);
            particle = go.GetComponent<ParticleSystem>();
            SetScalingModeHierarchy(particle);
            particle.Stop();
#endif
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            base.OnPlayableDestroy(playable);

            if (go != null)
            {
                if (notifyState != null) notifyState.previewGo = null;
                Object.DestroyImmediate(go);
                go = null;
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            if (particle == null) return;

            if (notifyState != null && go != null)
            {
                go.transform.localPosition = notifyState.position;
                go.transform.localRotation = notifyState.rotation;
                go.transform.localScale = notifyState.scale;
            }

            particle.Simulate((float)playable.GetTime());
        }

        // public override void OnBehaviourPlay(Playable playable, FrameData info)
        // {
        //     base.OnBehaviourPlay(playable, info);
        //     if (particle == null) return;
        //     if (!playable.GetGraph().IsPlaying()) return;
        //
        //     particle.Play();
        // }

        public static void SetScalingModeHierarchy(ParticleSystem ps)
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            foreach (var child in ps.GetComponentsInChildren<ParticleSystem>())
            {
                var childMain = child.main;
                childMain.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
        }

        private static void SetHideFlagsAll(GameObject gameObject, HideFlags hideFlags)
        {
            if (gameObject == null)
                return;

            gameObject.hideFlags = hideFlags;
            foreach (Transform child in gameObject.transform)
            {
                SetHideFlagsAll(child.gameObject, hideFlags);
            }
        }
    }
}