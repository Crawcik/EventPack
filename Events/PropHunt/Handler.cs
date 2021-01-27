using EventManager;

using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;

using System.Collections.Generic;

using UnityEngine;
using System.Linq;
using Smod2;

namespace PropHunt
{
    [Details("crawcik", 4, 2, 3, 9, "1.1")]
    partial class Handler : GameEvent, IEventHandlerPlayerPickupItem
    {
        private Dictionary<string, int> QueuePoints;
        private Dictionary<Player, Item> props;
        private List<Player> hunters;
        private Door SCP012;
        private ROUND_END_STATUS end_status = ROUND_END_STATUS.FORCE_END;
        //private int[] actualHunters;

        #region Settings
        public override void Register()
        {
            QueuePoints = new Dictionary<string, int>();
            props = new Dictionary<Player, Item>();
            hunters = new List<Player>();
            DefaultTranslation = new Dictionary<string, string>()
            {
                { "hunters_wait", "You will soon become hunter, find all props before decontamination!" },
                { "props_start", "You're now prop, try to hide somewhere before hunters find you!" },
                { "not_enough_players", "Cannot start event, not enough players!" }
            };
            DefaultConfig = new Dictionary<string, string>()
            {
                { "update_rate", "10" },
                { "hunters_percent", "20"},
                { "hunters_wait_time", "20" },
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
            previousUpdate = 0f;
            nextUpdate = 0f;
            unlockTime = Config<int>("hunters_wait_time");
            unlocked = false;

            props.Clear();
            hunters.Clear();
            end_status = ROUND_END_STATUS.ON_GOING;

            var all_players = ev.Server.GetPlayers();
            int hunters_count = Mathf.RoundToInt((Config<int>("hunters_percent") / 100f) * all_players.Count);
            if (hunters_count == 0)
            {
                ev.Server.Map.Broadcast(5, Translation("not_enough_players"), false);
                return;
            }
            foreach (Player player in all_players)
            {
                if(!QueuePoints.ContainsKey(player.UserId))
                    QueuePoints.Add(player.UserId, 0);
                QueuePoints[player.UserId]++;
            }
            var queue = (from player in QueuePoints orderby player.Value descending select player).ToList();
            var now_queue = queue.Where(x =>all_players.Select(y => y.UserId).Contains(x.Key)).ToArray();
            for (int i = 0; i < all_players.Count; i++)
            {
                if (i < hunters_count)
                {
                    Player hunter = all_players.Find(x => x.UserId == now_queue[0].Key);
                    if (hunter != null)
                        hunters.Add(hunter);
                }
            }
            foreach(Player player in hunters)
                QueuePoints[player.UserId] = 0;

            List<Door> doors = ev.Server.Map.GetDoors();
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_A").Locked = true;
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_B").Locked = true;
            SCP012 = doors.Find(x => x.Name == "012");
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
           
            foreach (Player player in all_players)
            {
                if (hunters.Contains(player))
                {
                    player.ChangeRole(RoleType.FACILITY_GUARD);
                    player.ClearInventory();
                    player.GiveItem(ItemType.MP7);
                    player.SetAmmo(AmmoType.AMMO9MM, 1000);
                    for (int i = 0; i < 7; i++)
                        player.GiveItem(ItemType.COIN);

                    player.PersonalBroadcast(30, Translation("hunters_wait"), false);
                }
                else
                {
                    player.PersonalBroadcast(30, Translation("props_start"), false);
                    player.ChangeRole(RoleType.CLASSD);
                    player.ClearInventory();
                    for (int i = 0; i < 7; i++)
                        player.GiveItem(ItemType.COIN);
                    try_again:
                    System.Array values = System.Enum.GetValues(typeof(Props));
                    ItemType itemtype = (ItemType)values.GetValue(Random.Range(0, values.Length));
                    var items = ev.Server.Map.GetItems(itemtype, true);
                    Item item = items[Random.Range(0, items.Count)];
                    if (props.Values.Contains(item))
                        goto try_again;
                    item.SetKinematic(false);
                    props.Add(player, item);
                }
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

        public void OnPlayerDropItem(PlayerDropItemEvent ev)
        {
            if (hunters.Exists(x => x.PlayerId == ev.Player.PlayerId))
                ev.Item.Remove();
        }
    }
}
