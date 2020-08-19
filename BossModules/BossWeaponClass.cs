using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace BossModules
{
    public class BossWeaponClass : MonoBehaviour
    {
        public delegate void OnUngrab(Handle handle, Interactor interactor, bool throwing);
        public static event OnUngrab OnUngrabEvent;

        public delegate void OnGrab(Handle handle, Interactor interactor);
        public static event OnGrab OnGrabPlayerEvent;
        public static event OnGrab OnGrabEnemyEvent;


        public delegate void OnCollision(ref CollisionStruct collisionInstance);
        public static event OnCollision OnCollisionPlayerHoldingEvent;
        public static event OnCollision OnCollisionEnemyHoldingEvent;
        public static event OnCollision OnCollisionNooneHolding;

        public bool playerHolding;
        public bool isHeld;

        public Item item;

        public virtual void Initialize(Item item)
        {
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;

            foreach (ColliderGroup colliderGroup in item.definition.colliderGroups)
            {
                colliderGroup.collisionHandler.OnCollisionStartEvent += CollisionHandler_OnCollisionStartEvent;
            }
            this.item = item;
        }

        private void CollisionHandler_OnCollisionStartEvent(ref CollisionStruct collisionInstance)
        {
            if (isHeld)
            {
                if (playerHolding)
                {
                    if (OnCollisionPlayerHoldingEvent != null)
                    {
                        OnCollisionPlayerHoldingEvent(ref collisionInstance);
                    }
                } else
                {
                    if (OnCollisionEnemyHoldingEvent != null)
                    {
                        OnCollisionEnemyHoldingEvent(ref collisionInstance);
                    }
                }
            } else
            {
                if (OnCollisionNooneHolding != null)
                {
                    OnCollisionNooneHolding(ref collisionInstance);
                }
            }
            
        }

        private void Item_OnUngrabEvent(Handle handle, Interactor interactor, bool throwing)
        {
            isHeld = false;
            playerHolding = false;
            if (OnUngrabEvent != null)
            {
                OnUngrabEvent(handle, interactor, throwing);
            }
            
        }

        private void Item_OnGrabEvent(Handle handle, Interactor interactor)
        {
            if (interactor.bodyHand.body.creature == Creature.player)
            {
                playerHolding = true;
                if (OnGrabPlayerEvent != null)
                {
                    OnGrabPlayerEvent(handle, interactor);
                }
                
            } else
            {
                playerHolding = false;

                if (OnGrabEnemyEvent != null)
                {
                    OnGrabEnemyEvent(handle, interactor);
                }
            }
        }
    }
}
