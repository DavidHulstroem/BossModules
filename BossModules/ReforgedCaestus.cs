using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;

namespace BossModules
{
    class ReforgedCaestus : ItemModule
    {
        public float coneAngle;
        public float coneLength;
        public string coneEffectId;
        public float coneForce;
        public float enemyBoostDuration;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ReforgedCaestusItem>().Initialize(item, this);
        }
    }

    class ReforgedCaestusItem : BossWeaponClass
    {
        public float coneAngle;
        public float coneLength;
        public float coneForce;
        public float enemyBoostDuration;

        private EffectData coneEffectData;

        public void Initialize(Item itemref, ReforgedCaestus module)
        {
            LoadBossWeapon(itemref);

            coneAngle = module.coneAngle;
            coneLength = module.coneLength;
            coneForce = module.coneForce;
            enemyBoostDuration = module.enemyBoostDuration;

            if (module.coneEffectId != "")
            {
                coneEffectData = Catalog.GetData<EffectData>(module.coneEffectId);
            }

            Item debug = Catalog.GetData<ItemPhysic>("DebugHand").Spawn();
            debug.transform.parent = item.transform;
            debug.transform.localPosition = Vector3.zero;
            debug.transform.localRotation = Quaternion.identity;

            OnCollisionCreaturePlayerHoldingEvent += ReforgedCaestusItem_OnCollisionCreaturePlayerHoldingEvent;
            OnCollisionCreatureEnemyHoldingEvent += ReforgedCaestusItem_OnCollisionCreatureEnemyHoldingEvent;
        }

        //Enemy
        private float lastPlayerHitTime;
        private bool isFast;

        private void ReforgedCaestusItem_OnCollisionCreatureEnemyHoldingEvent(ref CollisionStruct collisionInstance, Creature hitCreature)
        {
            if (hitCreature == Creature.player)
            {
                lastPlayerHitTime = Time.time;

                if (!isFast)
                {
                    isFast = true;
                    StartCoroutine(EnemyAttackSpeedCoroutine(itemHolderCreature));
                }
                
            }
        }

        IEnumerator EnemyAttackSpeedCoroutine(Creature creature)
        {
            //give speed and attackspeed
            creature.locomotion.speedMultiplier *= 2f;
            BrainHuman brain = creature.brain as BrainHuman;
            brain.attackMinMaxDelay *= 0.1f;
            brain.parryMinMaxDelay *= 0.1f;
            brain.parryEnabled = false;

            while (Time.time - lastPlayerHitTime < enemyBoostDuration)
            {
                yield return new WaitForEndOfFrame();
            }

            brain.attackMinMaxDelay /= 0.1f;
            creature.locomotion.speedMultiplier /= 2f;
            brain.parryMinMaxDelay /= 0.1f;
            brain.parryEnabled = true;
        }

        //Player

        private float lastColTime;

        private void ReforgedCaestusItem_OnCollisionCreaturePlayerHoldingEvent(ref CollisionStruct collisionInstance, Creature hitCreature)
        {
            Debug.Log("HitCreature");
            if (hitCreature != Creature.player)
            {
                Debug.Log("HitCreature no player");
                if (Time.time - lastColTime > 0.8f)
                {
                    Debug.Log("HitCreature last time");
                    lastColTime = Time.time;

                    //spawn effect
                    if (coneEffectData != null)
                    {
                        EffectInstance effectInstance = coneEffectData.Spawn(item.transform.position, item.transform.rotation);
                        effectInstance.Play();
                    }

                    Vector3 forwards = item.transform.forward;

                    //Want to make sure it is punching forwards
                    if (Vector3.Dot(collisionInstance.impactVelocity.normalized, forwards) > 0.4f)
                    {
                        Debug.Log("HitCreature is punching forwards");
                        //get creatures in front
                        foreach (Creature creature in Creature.list)
                        {
                            if (creature != Creature.player)
                            {
                                if (!IsBoss(creature))
                                {
                                    float distance = Vector3.Distance(creature.transform.position, item.transform.position);
                                    Vector3 direction = (Vector3.ProjectOnPlane(creature.transform.position, Vector3.up) - Vector3.ProjectOnPlane(collisionInstance.contactPoint, Vector3.up)).normalized;

                                    if (distance < coneLength)
                                    {
                                        Debug.Log("HitCreature is in range");
                                        if (Vector3.Dot(direction, forwards) > coneAngle)
                                        {
                                            Debug.Log("HitCreature in in angle");
                                            if (!creature.health.isKilled)
                                            {
                                                creature.ragdoll.SetState(Creature.State.Destabilized);
                                            }

                                            foreach (RagdollPart part in creature.ragdoll.parts)
                                            {
                                                Vector3 force = direction;
                                                part.rb.AddForce(force * coneForce * part.rb.mass, ForceMode.Impulse);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (!IsBoss(hitCreature))
                        {
                            //add force to the hit creature
                            if (!hitCreature.health.isKilled)
                            {
                                hitCreature.ragdoll.SetState(Creature.State.Destabilized);
                            }

                            foreach (RagdollPart part in hitCreature.ragdoll.parts)
                            {
                                Vector3 force = item.transform.forward;
                                part.rb.AddForce(force * coneForce * part.rb.mass, ForceMode.Impulse);
                            }
                        }
                    }
                }
            }
        }
    }
}
