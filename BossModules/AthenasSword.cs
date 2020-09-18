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
    public class AthenasSword : ItemModule
    {
        public float fireRainDuration;
        public float fireRainHeight;
        public float fireRainRadius;
        public float fireRainIntensity;

        public float fireLaserDelay;
        public float fireLaserVelocity;

        public string fireBallItemId;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            Debug.Log("AthenasSword Load");
            item.gameObject.AddComponent<AthenasSwordItem>().Initialize(item, this);
        }
    }

    public class AthenasSwordItem : BossWeaponClass
    {
        public float fireRainDuration;
        public float fireRainHeight;
        public float fireRainIntensity;
        public float fireRainRadius;

        public float fireLaserDelay;
        public float fireLaserVelocity;

        public ItemPhysic fireBallItemData;

        protected ItemPhysic projectileData;
        protected DamagerData projectileDamagerData;
        protected EffectData projectileDefectEffectData;
        protected EffectData projectileEffectData;

        private Transform swordTip;

        public void Initialize(Item item, AthenasSword module)
        {
            LoadBossWeapon(item);
            fireRainDuration = module.fireRainDuration;
            fireRainHeight = module.fireRainHeight;
            fireRainIntensity = module.fireRainIntensity;
            fireRainRadius = module.fireRainRadius;

            fireLaserDelay = module.fireLaserDelay;
            fireLaserVelocity = module.fireLaserVelocity;

            fireBallItemData = Catalog.GetData<ItemPhysic>(module.fireBallItemId);

            // Enemy
            OnCollisionCreatureEnemyHoldingEvent += CreatureCollisionEnemyHolding;

            //Player
            swordTip = item.definition.GetCustomReference("swordTip");
            if (!swordTip)
            {
                Debug.LogError("Weapon does not have swordTip reference, defaulting to flyref");
                swordTip = item.definition.flyDirRef;
            }
            OnHeldActionPlayer += OnHeldAction;
        }




        // Enemy

        private void CreatureCollisionEnemyHolding(ref CollisionStruct collisionInstance, Creature hitCreature)
        {
            if (hitCreature == Creature.player)
            {
                hitCreature.StartCoroutine(SpawnFireRain(hitCreature.body.transform.position, fireRainHeight, fireRainDuration, fireRainIntensity, fireRainRadius));
            }
        }

        IEnumerator SpawnFireRain(Vector3 groundPos, float rainHeight, float rainDuration, float intensity, float rainRadius)
        {
            Vector3 rainPos = groundPos + Vector3.up * rainHeight;
            float startTime = Time.time;

            while (Time.time - startTime < rainDuration)
            {
                Vector2 unitSphere = UnityEngine.Random.insideUnitCircle * rainRadius;
                Vector3 fireBallSpawnPos = rainPos + new Vector3(unitSphere.x, 0, unitSphere.y);

                SpawnFireRainBall(fireBallSpawnPos);

                yield return new WaitForSeconds(1 / intensity);
            }
        }

        private void SpawnFireRainBall(Vector3 pos)
        {
            Item ball = fireBallItemData.Spawn(true, null);
            ball.transform.position = pos;
            ball.transform.rotation = Quaternion.identity;

            foreach (CollisionHandler collisionHandler in ball.definition.collisionHandlers)
            {
                collisionHandler.SetPhysicModifier(this, 0, 5f, 1f, 1f, -1f, null);
            }

            ball.Throw(1f, Item.FlyDetection.Forced);

            StartCoroutine(DespawnOnHit(ball));
        }

        // Player

        private bool altActive;
        private float lastShotTime;

        private void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                altActive = true;
            } else if (action == Interactable.Action.AlternateUseStop)
            {
                altActive = false;
            }
        }

        private void Update()
        {
            if (altActive && playerHolding)
            {
                if (Time.time - lastShotTime > fireLaserDelay)
                {
                    Item ball = fireBallItemData.Spawn(true, null);
                    ball.transform.position = swordTip.position;
                    ball.transform.rotation = swordTip.rotation;

                    foreach (CollisionHandler collisionHandler in ball.definition.collisionHandlers)
                    {
                        collisionHandler.SetPhysicModifier(this, 0, 0f, 1f, -1f, -1f, null);
                    }

                    ball.IgnoreObjectCollision(item);


                    StartCoroutine(DespawnOnHit(ball));

                    ball.rb.velocity = swordTip.forward * fireLaserVelocity;
                    ball.Throw(1f, Item.FlyDetection.Forced);

                }
            } else {
                altActive = false;
            }
        }


        IEnumerator DespawnOnHit(Item refitem)
        {
            float startTime = Time.time;
            yield return new WaitUntil(() => Time.time - startTime > 2 || refitem.definition.collisionHandlers[0].isColliding);
            refitem.Despawn(0.1f);
        }
    }
}
