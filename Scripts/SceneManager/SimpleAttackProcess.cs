using System.Collections.Generic;
using JM.TimelineAction;
using UnityEngine;

namespace JM
{
    public static class SimpleAttackProcess
    {
        private static readonly Collider[] cols = new Collider[32];

        public static void RoleAttack(SimpleRoleCtrl role, BoxNotifyState box,
            HashSet<SimpleMonsterCtrl> alreadyHit = null)
        {
            int count = PhysicsOverlap(box, role.transform, LayerMask.GetMask("Monster"));

            bool hitMonster = false;
            SimpleMonsterCtrl lastHitMonster = null;
            for (int i = 0; i < count; i++)
            {
                var col = cols[i];
                if (col.TryGetComponent<SimpleMonsterCtrl>(out var monsterCtrl))
                {
                    if (alreadyHit != null && !alreadyHit.Add(monsterCtrl))
                        continue;

                    hitMonster = true;
                    lastHitMonster = monsterCtrl;
                    float damage = box.hitId;
                    monsterCtrl.TakeDamage(damage, role.transform.position, box.hitStrength, box.hitGatherDist, box.knockbackDuration);
                    monsterCtrl.TriggerHitstop(box.hitstopDuration, box.hitstopSpeed).Forget();
                }
            }

            if (hitMonster)
            {
                role.TriggerHitstop(box.hitstopDuration, box.hitstopSpeed).Forget();

                if (box.hitAudio)
                {
                    AudioMgr.Instance.PlaySFX(box.hitAudio, lastHitMonster.transform.position);
                }

                if (box.particlePrefab)
                {
                    var vfxWPos = lastHitMonster.transform.position + new Vector3(0, 1f, 0);
                    if (box.setParticleRot)
                    {
                        ParticleMgr.Instance.PlayAt(box.particlePrefab, vfxWPos, role.transform.forward,
                            box.rotValue, box.rotMaxValue);
                    }
                    else
                    {
                        ParticleMgr.Instance.PlayAt(box.particlePrefab, vfxWPos);
                    }
                }
            }
        }

        public static void MonsterAttack(SimpleMonsterCtrl monster, BoxNotifyState box,
            HashSet<SimpleRoleCtrl> alreadyHit = null)
        {
            int count = PhysicsOverlap(box, monster.transform, LayerMask.GetMask("Character"));

            for (int i = 0; i < count; i++)
            {
                var col = cols[i];
                if (col.TryGetComponent<SimpleRoleCtrl>(out var roleCtrl))
                {
                    if (alreadyHit != null && !alreadyHit.Add(roleCtrl))
                        continue;

                    float damage = box.hitId;
                    roleCtrl.HandleMonsterAttackRole(damage, monster.transform.position, box.hitStrength);
                }
            }
        }

        private static int PhysicsOverlap(BoxNotifyState box, Transform parent, int layerMask)
        {
            var worldPos = parent.TransformPoint(box.center);
            if (box.boxShape == EBoxShape.Sphere)
            {
                return Physics.OverlapSphereNonAlloc(worldPos, box.radius, cols, layerMask);
            }
            if (box.boxShape == EBoxShape.Box)
            {
                return Physics.OverlapBoxNonAlloc(worldPos, box.size / 2, cols, parent.rotation, layerMask);
            }
            return 0;
        }
    }
}
