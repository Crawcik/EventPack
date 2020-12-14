using EventManager;

using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;

using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

namespace PropHunt
{
    [Details("crawcik", 4, 2, 3, 9, "1.1")]
    class Handler : GameEvent, IEventHandlerDisableStatusEffect, IEventHandlerPlayerPickupItemLate
    {
        //private Dictionary<string, int> QueuePoints = new Dictionary<string, int>();
        private List<Task> tasks = new List<Task>();
        private ROUND_END_STATUS end_status = ROUND_END_STATUS.FORCE_END;
        //private int[] actualHunters;

        #region Settings
        public override void Register()
        {
            DefaultTranslation = new Dictionary<string, string>()
            {
                { "hunters_wait", "You will soon become hunter, find them all before decontamination!" },
                { "props_start", "You're now prop, try to hide somewhere before hunters find you!" }
            };
        }

        public override string[] GetCommands() => new[] { "prop", "hunt", "ph", "prop_hunt" };

        public override string GetName() => "Prop Hunt";
        #endregion

        public override void EventStart(RoundStartEvent ev)
        {
            end_status = ROUND_END_STATUS.ON_GOING;
            foreach(Player player in ev.Server.GetPlayers())
            {
                player.ChangeRole(RoleType.CLASSD);
                player.GiveItem(ItemType.SCP268);
                player.GetPlayerEffect(StatusEffect.SCP268).Enable(60f);
            }
            List<Door> doors = ev.Server.Map.GetDoors();
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_A").Locked = true;
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_B").Locked = true;
            foreach (Door door in doors)
            {
                Vector vec = door.Position;
                int rng = Random.Range(0, 6);
                for (int i = 0; i < rng; i++)
                {
                    float x = Random.Range(vec.x - 15f, vec.x + 15f);
                    float z = Random.Range(vec.z - 15f, vec.z + 15f);
                    float y = vec.y + 1f;
                    Vector spanw_pos = new Vector(x, y, z);

                    System.Array values = System.Enum.GetValues(typeof(Props));
                    ItemType item = (ItemType)values.GetValue(Random.Range(0, values.Length));
                    ev.Server.Map.SpawnItem(item, spanw_pos, Vector.Zero);
                }
            }
           
            foreach (Player player in ev.Server.GetPlayers())
            {
                player.PersonalBroadcast(30, Translation("props_start"), false);
                System.Array values = System.Enum.GetValues(typeof(Props));
                ItemType itemtype = (ItemType)values.GetValue(Random.Range(0, values.Length));
                Item item = ev.Server.Map.GetItems(itemtype, true)[0];
                tasks.Add(Follow(player, item));
            }
            tasks.ForEach(x => x.GetAwaiter());
        }

        private async Task Follow(Player player, Item item)
        {
            item.SetKinematic(false);
            while (player.TeamRole.Role == RoleType.CLASSD || end_status != ROUND_END_STATUS.ON_GOING)
            {
                if (Vector.Distance(item.GetPosition(), player.GetPosition()) > 1f)
                {
                    if (item.GetKinematic())
                        item.SetKinematic(false);
                    Vector vector = new Vector(player.GetPosition().x, player.GetPosition().y - 0.5f, player.GetPosition().z);
                    item.SetPosition(vector);
                }
                else if (!item.GetKinematic())
                    item.SetKinematic(true);
                await Task.Delay(1000 / 8);
            }
        }

        public override void EventEnd(RoundEndEvent ev) => end_status = ev.Status;

        public void OnDisableStatusEffect(DisableStatusEffectEvent ev)
        {
            if (!ev.Player.HasItem(ItemType.SCP268))
                ev.Player.GiveItem(ItemType.SCP268);
            ev.Player.GetPlayerEffect(StatusEffect.SCP268).Enable(60f);
        }

        public void OnPlayerPickupItemLate(PlayerPickupItemLateEvent ev)
        {
            ev.Item.Drop();
        }
    }
}
