using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace BossModules
{
    public class HadesRam : ItemModule
    {
        public float playerHitForce;
        public float aoeRange;
        public float aoeForce;
        public string aoeEffectId;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            Debug.Log("HadesRam Load");
            item.gameObject.AddComponent<HadesRamItem>().Initialize(item, this);
        }
    }

    class HadesRamItem : BossWeaponClass
    {
        public float playerHitForce;
        public float aoeRange;
        public float aoeForce;


        private EffectData aoeEffectData;

        public void Initialize(Item itemref, HadesRam module)
        {
            LoadBossWeapon(itemref);

            playerHitForce = module.playerHitForce;
            aoeRange = module.aoeRange;
            aoeForce = module.aoeForce;
            if (module.aoeEffectId != "")
            {
                aoeEffectData = Catalog.GetData<EffectData>(module.aoeEffectId);
            }

            OnCollisionPlayerHoldingEvent += CollisionPlayerHolding;
            OnCollisionCreatureEnemyHoldingEvent += CollisionEnemyHolding;
        }

        private void CollisionEnemyHolding(ref CollisionStruct collisionInstance, Creature hitCreature)
        {
            if (hitCreature != Creature.player)
            {
                if (!hitCreature.health.isKilled)
                {
                    hitCreature.ragdoll.SetState(Creature.State.Destabilized);
                    foreach (RagdollPart part in hitCreature.ragdoll.parts)
                    {
                        part.rb.AddForce(-collisionInstance.contactNormal * playerHitForce * part.rb.mass, ForceMode.Impulse);
                    }
                }
                    
            } else
            {
                Debug.Log("Enemy Hit Player");
                Player.local.locomotion.isGrounded = false;
                Player.local.locomotion.rb.AddForce(-collisionInstance.contactNormal * playerHitForce + Vector3.up * playerHitForce * 0.5f, ForceMode.Impulse);
            }
           
        }

        //Player

        private float lastColTime;

        private void CollisionPlayerHolding(ref CollisionStruct collisionInstance)
        {
            if (collisionInstance.impactVelocity.magnitude > 2)
            {
                if (!collisionInstance.targetCollider.attachedRigidbody)
                {
                    if (Time.time - lastColTime > 1f)
                    {
                        lastColTime = Time.time;

                        if (aoeEffectData != null)
                        {
                            EffectInstance effect = aoeEffectData.Spawn(collisionInstance.contactPoint, Quaternion.LookRotation(collisionInstance.contactNormal));
                            effect.Play();
                        }

                        foreach (Creature creature in Creature.list)
                        {
                            if (Vector3.Distance(creature.body.transform.position, collisionInstance.contactPoint) < aoeRange)
                            {
                                if (creature == Creature.player)
                                {
                                    continue;
                                }
                                if (!creature.health.isKilled)
                                {
                                    if (IsBoss(creature))
                                    {
                                        Debug.Log("Ram is boss");
                                        creature.TryAction(new ActionStagger((creature.body.transform.position - collisionInstance.contactPoint).normalized, 10f, ActionStagger.Type.FallGround), true);
                                        continue;
                                    }
                                    else
                                    {
                                        creature.ragdoll.SetState(Creature.State.Destabilized);
                                    }
                                }

                                foreach (RagdollPart part in creature.ragdoll.parts)
                                {
                                    part.rb.AddExplosionForce(aoeForce * part.rb.mass, collisionInstance.contactPoint, aoeRange, 2f, ForceMode.Impulse);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
