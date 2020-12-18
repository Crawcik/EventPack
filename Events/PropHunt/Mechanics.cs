using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using UnityEngine;

namespace PropHunt
{
    partial class Handler : IEventHandlerFixedUpdate
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
                foreach (var pair in props)
                    UpdateProp(pair.Key, pair.Value);
            }
            if (!unlocked && previousUpdate >= unlockTime)
            {
                hunters.ForEach(x => x.Teleport(SCP012.Position));
                unlocked = true;
            }
            previousUpdate += Time.fixedDeltaTime;
        }

        // Update is called once per second
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
            if (Vector.Distance(item.GetPosition(), player.GetPosition()) > 1f)
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
                player.GetPlayerEffect(StatusEffect.SCP268).Enable(15f);
            }
            if (player.GetPlayerEffect(StatusEffect.AMNESIA).Duration <= 0f)
                player.GetPlayerEffect(StatusEffect.AMNESIA).Enable(15f);
        }
    }
}
