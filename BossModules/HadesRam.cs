using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace BossModules
{
    class HadesRam : ItemModule
    {
        public float playerHitForce;
        public float aoeRange;
        public float aoeForce;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<HadesRamItem>().Initialize(item, this);
        }
    }

    class HadesRamItem : BossWeaponClass
    {
        public float playerHitForce;
        public float aoeRange;
        public float aoeForce;

        public void Initialize(Item item, HadesRam module)
        {
            base.Initialize(item);

            playerHitForce = module.playerHitForce;
            aoeRange = module.aoeRange;
            aoeForce = module.aoeForce;

            OnCollisionPlayerHoldingEvent += CollisionPlayerHolding;
            OnCollisionPlayerHoldingEvent += CollisionEnemyHolding;
        }

        private void CollisionEnemyHolding(ref CollisionStruct collisionInstance)
        {
            if (collisionInstance.damageStruct.hitRagdollPart)
            {
                Creature creature = collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature;

                if (creature != Creature.player)
                {
                    if (!creature.health.isKilled)
                    {
                        creature.ragdoll.SetState(Creature.State.Destabilized);
                    }
                    
                } else
                {
                    Player.local.locomotion.rb.AddForce(-collisionInstance.contactNormal * playerHitForce, ForceMode.Impulse);
                }
            }
        }

        private void CollisionPlayerHolding(ref CollisionStruct collisionInstance)
        {
            List<Collider> colliders = Physics.OverlapSphere(collisionInstance.contactPoint, aoeRange).ToList();

            foreach (Collider collider in colliders)
            {
                if (collider.attachedRigidbody)
                {
                    Rigidbody rb = collider.attachedRigidbody;
                    if (rb.GetComponentInParent<Creature>())
                    {
                        Creature creature = rb.GetComponentInParent<Creature>();
                        if (rb.GetComponentInParent<Creature>() == Creature.player)
                        {
                            continue;
                        }
                        creature.ragdoll.SetState(Creature.State.Destabilized);
                    }

                    rb.AddExplosionForce(aoeForce, collisionInstance.contactPoint, aoeRange);
                }
            }
        }
    }
}
