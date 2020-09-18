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
    public class WeaponAutoScalerModule : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<WeaponAutoScaler>().Initialize(item, this);
        }
    }

    public class WeaponAutoScaler : BossWeaponClass
    {
        private bool grabFix;
        public void Initialize(Item item, WeaponAutoScalerModule module)
        {
            LoadBossWeapon(item);
            OnGrabPlayerEvent += ItemGrabbedEvent;
            OnGrabEnemyEvent += ItemGrabbedEvent;
        }

        private void ItemGrabbedEvent(Handle handle, Interactor interactor)
        {
            float height = interactor.bodyHand.body.creature.umaCharacter.GetDnaValue("height");
            if (interactor.bodyHand.body.creature.umaCharacter.GetDnaValue("height") != 0.5f)
            {
                item.transform.localScale = Vector3.one * height / 0.5f;
            }

            if (!grabFix)
            {
                StartCoroutine(RegrabHandle(interactor, handle));
                grabFix = true;
            }             
        }

        IEnumerator RegrabHandle(Interactor interactor, Handle handle)
        {
            handle.Release();
            yield return new WaitForSeconds(0.1f);
            interactor.Grab(handle);
            yield return new WaitForSeconds(0.1f);
            grabFix = false;
        }
    }
}
