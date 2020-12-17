using EventManager;

using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;

using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using System.Linq;

namespace PropHunt
{
    [Details("crawcik", 4, 2, 3, 9, "1.1")]
    partial class Handler : GameEvent, IEventHandlerPlayerPickupItem
    {
        //private Dictionary<string, int> QueuePoints = new Dictionary<string, int>();
        private Dictionary<Player,Item> props = new Dictionary<Player, Item>();
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
            DefaultConfig = new Dictionary<string, string>()
            {
                { "update_rate", "10" },
                { "min_items_in_room", "2" },
                { "max_items_in_room", "6" },
            };
        }

        public override string[] GetCommands() => new[] { "prop", "hunt", "ph", "prop_hunt" };

        public override string GetName() => "Prop Hunt";
        #endregion

        public override void EventStart(RoundStartEvent ev)
        {
            updateRate = 1f / Config<int>("update_rate");

            props.Clear();
            end_status = ROUND_END_STATUS.ON_GOING;
            foreach(Player player in ev.Server.GetPlayers())
            {
                player.ChangeRole(RoleType.CLASSD);
                player.ClearInventory();
                for (int i = 0; i < 7; i++)
                    player.GiveItem(ItemType.COIN);
            }
            List<Door> doors = ev.Server.Map.GetDoors();
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_A").Locked = true;
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_B").Locked = true;
            foreach (Door door in doors)
            {
                Vector vec = door.Position;
                int rng = Random.Range(Config<int>("min_items_in_room"), Config<int>("max_items_in_room"));
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
                try_again:
                System.Array values = System.Enum.GetValues(typeof(Props));
                ItemType itemtype = (ItemType)values.GetValue(Random.Range(0, values.Length));
                var items = ev.Server.Map.GetItems(itemtype, true);
                Item item = items[Random.Range(0,items.Count)];
                if (props.Values.Contains(item))
                    goto try_again;
                item.SetKinematic(false);
                props.Add(player, item);
            }
        }

        public override void EventEnd(RoundEndEvent ev)
        {
            end_status = ev.Status;
        }

        public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
        {
            if (ev.Item.ItemType == ItemType.SCP268)
                ev.Item.Remove();
            else
                ev.Item.Drop();
        }
    }
}
