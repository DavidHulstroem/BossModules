using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace BossModules
{
    class AssassinDarkDagger : ItemModule
    {
        public float enemyInvisDuration;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<AssassinDarkDaggerItem>().Initialize(item, this);
        }
    }

    class AssassinDarkDaggerItem : BossWeaponClass
    {
        private bool EnemyInvis;

        public float enemyInvisDuration;

        public void Initialize(Item itemref, AssassinDarkDagger module)
        {
            LoadBossWeapon(itemref);

            enemyInvisDuration = module.enemyInvisDuration;


            OnHeldActionPlayer += ActionKeyPressed;

            OnCollisionPlayerHoldingEvent += ItemCollision;

            OnUngrabEvent += UngrabEvent;

            //Enemy

            OnCollisionCreatureEnemyHoldingEvent += AssassinDarkDaggerItem_OnCollisionCreatureEnemyHoldingEvent;
        }

        private void AssassinDarkDaggerItem_OnCollisionCreatureEnemyHoldingEvent(ref CollisionStruct collisionInstance, Creature hitCreature)
        {
            Debug.Log("Enemy hit a creature with dark dagger");
            if (hitCreature == Creature.player)
            {
                if (!EnemyInvis)
                {
                    StartCoroutine(EnemyInvisCoroutine(itemHolderCreature));
                    EnemyInvis = true;
                }
            }
        }

        IEnumerator EnemyInvisCoroutine(Creature creature)
        {
            float startTime = Time.time;

            //give speed and attackspeed
            creature.locomotion.speedMultiplier *= 2f;
            BrainHuman brain = creature.brain as BrainHuman;
            brain.attackMinMaxDelay *= 0.1f;
            brain.parryMinMaxDelay *= 0.1f;
            brain.parryEnabled = false;

            //Make enemy invis and items
            while (Time.time - startTime < enemyInvisDuration)
            {
                foreach (Renderer renderer in creature.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }
                yield return new WaitForEndOfFrame();
            }

            brain.attackMinMaxDelay /= 0.1f;
            creature.locomotion.speedMultiplier /= 2f;
            brain.parryMinMaxDelay /= 0.1f;
            brain.parryEnabled = true;

            EnemyInvis = false;
        }



        private void UngrabEvent(Handle handle, Interactor interactor, bool throwing)
        {
            if (bossPlayer.isInvis)
            {
                StopInvis();
            }
        }

        private void ItemCollision(ref CollisionStruct collisionInstance)
        {
            if (bossPlayer.isInvis)
            {
                StopInvis();
            }   
        }

        private void ActionKeyPressed(Interactor interactor, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                if (!bossPlayer.isInvis)
                {
                    BecomeInvisible();
                }
            }
        }

        private void BecomeInvisible()
        {
            bossPlayer.isInvis = true;
            StartCoroutine(InvisibilityCoroutine(Creature.player));

        }

        private void StopInvis()
        {
            bossPlayer.isInvis = false;
            bossPlayer.vignette.color.value = Color.black;
            bossPlayer.vignette.intensity.value = 0f;
        }

        private void LateUpdate()
        {
            if (bossPlayer.isInvis)
            {
                bossPlayer.vignette.color.value = new Color(100 / 220, 0 / 220, 255 / 220);
                bossPlayer.vignette.intensity.value = 1.5f;
            }
        }

        public IEnumerator InvisibilityCoroutine(Creature creature)
        {

            while (bossPlayer.isInvis)
            {
                if (creature == Creature.player)
                {
                    foreach (Creature creature2 in Creature.list)
                    {
                        if (creature2 != Creature.player && !creature2.health.isKilled && creature2.factionId != -1)
                        {
                            StartCoroutine(ChangeFaction(creature2));
                        }
                    }
                }
                yield return new WaitForSeconds(0.2f);
            }
        }

        public IEnumerator ChangeFaction(Creature creature)
        {
            int fac = creature.factionId;
            creature.SetFaction(-1);
            BrainHuman brain = creature.brain as BrainHuman;
            brain.canLeave = false;
            yield return new WaitUntil(() => !bossPlayer.isInvis);
            creature.SetFaction(fac);
            brain.canLeave = true;
        }
    }
}
