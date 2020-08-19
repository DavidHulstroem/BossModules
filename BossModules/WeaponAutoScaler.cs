using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace BossModules
{
    class WeaponAutoScalerModule : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<WeaponAutoScaler>().Initialize(item);
        }
    }

    class WeaponAutoScaler : BossWeaponClass
    {

        public override void Initialize(Item item)
        {
            base.Initialize(item);
            OnGrabPlayerEvent += ItemGrabbedEvent;
            OnGrabEnemyEvent += ItemGrabbedEvent;
        }

        private void ItemGrabbedEvent(Handle handle, Interactor interactor)
        {
            if (interactor.bodyHand.body.creature.transform.localScale != Vector3.one)
            {
                item.transform.localScale = interactor.bodyHand.body.creature.transform.localScale;
            }
        }
    }
}
