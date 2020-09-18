using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BossModules
{
    public class BossWeaponClass : MonoBehaviour
    {
        protected delegate void OnUngrab(Handle handle, Interactor interactor, bool throwing);
        protected event OnUngrab OnUngrabEvent;

        protected delegate void OnGrab(Handle handle, Interactor interactor);
        protected event OnGrab OnGrabPlayerEvent;
        protected event OnGrab OnGrabEnemyEvent;


        protected delegate void OnCollision(ref CollisionStruct collisionInstance);
        protected event OnCollision OnCollisionPlayerHoldingEvent;
        protected event OnCollision OnCollisionEnemyHoldingEvent;
        protected event OnCollision OnCollisionNooneHolding;

        protected delegate void OnCollisionCreature(ref CollisionStruct collisionInstance, Creature hitCreature);
        protected event OnCollisionCreature OnCollisionCreaturePlayerHoldingEvent;
        protected event OnCollisionCreature OnCollisionCreatureEnemyHoldingEvent;
        protected event OnCollisionCreature OnCollisionCreatureNooneHoldingEvent;

        protected delegate void OnHeldAtion(Interactor interactor, Handle handle, Interactable.Action action);
        protected event OnHeldAtion OnHeldActionPlayer;

        protected bool playerHolding;
        protected bool isHeld;

        protected Creature itemHolderCreature;

        protected Item item;

        protected BossPlayerClass bossPlayer;

        protected void LoadBossWeapon(Item itemref)
        {
            if (Creature.player.gameObject.GetComponent<BossPlayerClass>())
            {
                bossPlayer = Creature.player.gameObject.GetComponent<BossPlayerClass>();
            } else
            {
                bossPlayer = Creature.player.gameObject.AddComponent<BossPlayerClass>();
            }

            itemref.OnGrabEvent += Item_OnGrabEvent;
            itemref.OnUngrabEvent += Item_OnUngrabEvent;
            itemref.OnHeldActionEvent += Item_OnHeldActionEvent;
            foreach (CollisionHandler collisionHandler in itemref.definition.collisionHandlers)
            {
                collisionHandler.OnCollisionStartEvent += CollisionHandler_OnCollisionStartEvent;                
            }
            item = itemref;
        }

        private void Item_OnHeldActionEvent(Interactor interactor, Handle handle, Interactable.Action action)
        {
            if (playerHolding)
            {
                if (OnHeldActionPlayer != null)
                {
                    OnHeldActionPlayer(interactor, handle, action);
                }
            } 
        }

        private void CollisionHandler_OnCollisionStartEvent(ref CollisionStruct collisionInstance)
        {
            if (isHeld)
            {
                //Debug.Log("col is held");
                if (playerHolding)
                {
                    //Debug.Log("col player is holding");
                    if (OnCollisionPlayerHoldingEvent != null)
                    {
                        OnCollisionPlayerHoldingEvent(ref collisionInstance);
                    }

                    if (collisionInstance.damageStruct.hitRagdollPart)
                    {
                        Creature creature = collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature;
                        if (OnCollisionCreaturePlayerHoldingEvent != null)
                        {
                            OnCollisionCreaturePlayerHoldingEvent(ref collisionInstance, creature);
                        }
                    }

                } else
                {
                    //Debug.Log("col player is not holding");
                    if (OnCollisionEnemyHoldingEvent != null)
                    {
                        OnCollisionEnemyHoldingEvent(ref collisionInstance);
                    }

                    if (collisionInstance.damageStruct.hitRagdollPart)
                    {
                        Creature creature = collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature;
                        if (OnCollisionCreatureEnemyHoldingEvent != null)
                        {
                            OnCollisionCreatureEnemyHoldingEvent(ref collisionInstance, creature);
                        }
                    }
                }
            } else
            {
                //Debug.Log("col noone is holding");
                if (OnCollisionNooneHolding != null)
                {
                    OnCollisionNooneHolding(ref collisionInstance);
                }

                if (collisionInstance.damageStruct.hitRagdollPart)
                {
                    Creature creature = collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature;
                    if (OnCollisionCreatureNooneHoldingEvent != null)
                    {
                        OnCollisionCreatureNooneHoldingEvent(ref collisionInstance, creature);
                    }
                }
            }
            
        }

        private void Item_OnUngrabEvent(Handle handle, Interactor interactor, bool throwing)
        {
            //Check if it is still being held
            if (handle.item.handlers.Count <= 0)
            {
                isHeld = false;
                playerHolding = false;
                itemHolderCreature = null;

                if (OnUngrabEvent != null)
                {
                    OnUngrabEvent(handle, interactor, throwing);
                }
            }

           
            
        }

        private void Item_OnGrabEvent(Handle handle, Interactor interactor)
        {
            //Debug.Log("Item grabbed");

            if (!isHeld)
            {
                isHeld = true;

                itemHolderCreature = interactor.bodyHand.body.creature;

                if (interactor.bodyHand.body.creature == Creature.player)
                {
                    //Debug.Log("Item grabbed Player");
                    playerHolding = true;
                    if (OnGrabPlayerEvent != null)
                    {
                        OnGrabPlayerEvent(handle, interactor);
                    }

                }
                else
                {
                    //Debug.Log("Item grabbed Enemy");
                    playerHolding = false;

                    if (OnGrabEnemyEvent != null)
                    {
                        OnGrabEnemyEvent(handle, interactor);
                    }
                }
            }
 
        }

        public static bool IsBoss(Creature creature)
        {
            if (creature.health.maxHealth > 10000)
            {
                return true;
            } else
            {
                return false;
            }
        }
    }

    public class BossPlayerClass : MonoBehaviour
    {
        public Vignette vignette;
        public bool isInvis;

        private void Awake()
        {
            Volume ppmVolume = PostProcessManager.local.GetComponent<Volume>();
            ppmVolume.profile.TryGet<Vignette>(out vignette);
        }
    }
}
