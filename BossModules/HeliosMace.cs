using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BossModules
{
    class HeliosMace : ItemModule
    {
        public float playerFireDamage;
        public float playerDelay;
        public float playerFireDuration;

        public float enemyFireDamage;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<HeliosMaceItem>().Initialize(item, this);
        }
    }

    class HeliosMaceItem : BossWeaponClass
    {
        public float playerFireDamage;
        public float playerDelay;
        public float playerFireDuration;

        public float enemyFireDamage;

        public float lastPlayerUseTime;



        public void Initialize(Item refitem, HeliosMace module)
        {
            LoadBossWeapon(refitem);

            //Player
            playerFireDamage = module.playerFireDamage;
            playerDelay = module.playerDelay;
            playerFireDuration = module.playerFireDuration;

            OnCollisionPlayerHoldingEvent += OnCollisionPlayer;

            //Enemy
            enemyFireDamage = module.enemyFireDamage;

            OnUngrabEvent += HeliosMaceItem_OnUngrabEvent;

        }

        private void HeliosMaceItem_OnUngrabEvent(Handle handle, Interactor interactor, bool throwing)
        {
            if (interactor.bodyHand.body.creature != Creature.player)
            {
                bossPlayer.vignette.color.value = Color.black;
                bossPlayer.vignette.intensity.value = 0f;
            }
        }

        float lastMoveTime;
        float lastPlayerDamageTime;

        private void Update()
        {
            if (Player.local.locomotion.velocity.magnitude > 0.02f)
            {
                lastMoveTime = Time.time;                
            }

            if (!playerHolding && isHeld)
            {
                float emissionPow = 1.7f;

                float perlin = Mathf.PerlinNoise(Time.time, Time.time);
                bossPlayer.vignette.color.value = Color.Lerp(new Color(1f * emissionPow, 0.3f * emissionPow, 0f * emissionPow), new Color(1f * emissionPow, 0.067f * emissionPow, 0f * emissionPow), perlin);
                bossPlayer.vignette.intensity.value = 1f * Mathf.Clamp(perlin, 0.8f, 1f);

                if (Time.time - lastMoveTime > 0.1)
                {
                    //Player is standing still
                    if (Player.local.locomotion.isGrounded)
                    {
                        //Player is on ground
                        BurnThePlayer();
                    }
                }
            }
        }

        private void BurnThePlayer()
        {
            if (Time.time - lastPlayerDamageTime > 0.5f)
            {
                lastPlayerDamageTime = Time.time;
                if (!GameManager.local.playerIsInvincible)
                {
                    Creature.player.health.currentHealth -= enemyFireDamage;
                    if (Creature.player.health.currentHealth <= 0)
                    {
                        Creature.player.health.Kill();
                    }
                }
                
                
            }
        }


        // Player

        private void OnCollisionPlayer(ref CollisionStruct collisionInstance)
        {
            if (Time.time - lastPlayerUseTime > playerDelay)
            {
                lastPlayerUseTime = Time.time;

                if (!collisionInstance.targetCollider.attachedRigidbody && collisionInstance.impactVelocity.magnitude > 5)
                {
                    foreach (Creature creature in Creature.list)
                    {
                        if (IsBoss(creature))
                        {
                            continue;
                        }

                        if (creature != Creature.player && !creature.health.isKilled)
                        {
                            creature.ragdoll.SetState(Creature.State.Destabilized);
                            creature.StartCoroutine(Burn(creature, playerFireDuration, playerFireDamage));
                        }
                    }
                }
            }
        }

        public IEnumerator Burn(Creature creature, float duration, float fireDamage)
        {
            float startTime = Time.time;

            creature.TryAction(new ActionShock(10f, duration), true);

            while (Time.time - startTime > duration)
            {
                CollisionStruct collisionStruct = new CollisionStruct(new DamageStruct(DamageType.Energy, fireDamage));
                creature.health.Damage(ref collisionStruct);
                yield return new WaitForSeconds(1f);
            }
        }

        // Enemy
    }

    public class ActionBurn : ThunderRoad.ActionShock
    {
        public float fireDamage;

        public ActionBurn(float force, float duration, float fireDamage, EffectData effectData = null)
        {
            this.force = force;
            this.duration = duration;
            this.effectData = effectData;
            this.fireDamage = fireDamage;
        }

        
    }
}
