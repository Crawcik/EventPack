using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using UnityEngine;

namespace PropHunt
{
    partial class Handler : IEventHandlerFixedUpdate, IEventHandlerShoot
    {
        private float updateRate = 0.1f,
            previousUpdate = 0f,
            nextUpdate = 0f,
            unlockTime = 20f;
        private bool unlocked = false;
        public void OnFixedUpdate(FixedUpdateEvent ev)
        {
            if (previousUpdate >= nextUpdate)
            {
                nextUpdate = previousUpdate + updateRate;
                lock (props)
                {
                    foreach (var pair in props)
                        UpdateProp(pair.Key, pair.Value);
                }
                lock (hunters)
                {
                    foreach (Player hunter in hunters)
                        UpdateHunter(hunter);
                }
            }
            if (!unlocked && previousUpdate >= unlockTime)
            {
                hunters.ForEach(x => x.Teleport(new Vector(SCP012.Position.x, SCP012.Position.y + 0.7f, SCP012.Position.z)));
                SCP012.Open = true;
                unlocked = true;
            }
            previousUpdate += Time.fixedDeltaTime;
        }

        private void UpdateProp(Player player, Item item)
        {
            if (player == null)
            {
                props.Remove(player);
                return;
            }
            if(!(player.TeamRole.Role == RoleType.CLASSD && end_status == ROUND_END_STATUS.ON_GOING))
            {
                props.Remove(player);
                return;
            }
            if (!item.InWorld)
            {
                props.Remove(player);
                player.Kill();
                return;
            }
            if (Vector.Distance(item.GetPosition(), player.GetPosition()) > 1.3f)
            {
                if (item.GetKinematic())
                    item.SetKinematic(false);
                Vector vector = new Vector(player.GetPosition().x, player.GetPosition().y - 0.6f, player.GetPosition().z);
                item.SetPosition(vector);
            }
            else if (!item.GetKinematic())
                item.SetKinematic(true);
            if (player.GetPlayerEffect(StatusEffect.SCP268).Duration <= 0f)
            {
                if (player.HasItem(ItemType.SCP268))
                    player.GetInventory().Find(x => x.ItemType == ItemType.SCP268).Remove();
                player.GiveItem(ItemType.SCP268);
                player.GetPlayerEffect(StatusEffect.SCP268).Enable(14f);
            }
            if (player.GetPlayerEffect(StatusEffect.AMNESIA).Duration <= 0f)
                player.GetPlayerEffect(StatusEffect.AMNESIA).Enable(14f);
        }

        private void UpdateHunter(Player player)
        {
            if (player == null)
            {
                hunters.Remove(player);
                return;
            }
            if (!(player.TeamRole.Role == RoleType.FACILITY_GUARD && end_status == ROUND_END_STATUS.ON_GOING))
            {
                hunters.Remove(player);
                return;
            }
            int i = player.GetInventory().Count;
            if(i>0)
            {
                _ = player.HasItem(ItemType.MP7) ? player.GiveItem(ItemType.COIN) : player.GiveItem(ItemType.MP7);
                i--;
            }
            if(player.GetAmmo(AmmoType.AMMO9MM) > 0)
                player.SetAmmo(AmmoType.AMMO9MM, 1000);


        }

        public void OnShoot(PlayerShootEvent ev)
        {
            if (ev.TargetHitbox == HitBoxType.NULL)
                ev.Player.HP -= 1f;
            else
                ev.Player.HP = 100f;
        }
    }
}
