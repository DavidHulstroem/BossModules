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
        public bool playerHolding;
        public bool isHeld;

        public virtual void Initialize(Item item)
        {
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;
        }

        private void Item_OnUngrabEvent(Handle handle, Interactor interactor, bool throwing)
        {
            isHeld = false;
            playerHolding = false;
        }

        private void Item_OnGrabEvent(Handle handle, Interactor interactor)
        {
            if (interactor.bodyHand.body.creature == Creature.player)
            {
                playerHolding = true;
            } else
            {
                playerHolding = false;
            }
        }
    }
}
