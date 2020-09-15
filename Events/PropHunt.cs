using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManager.Events
{
    public class PropHunt : Event, IEventHandlerPlayerPickupItem, IEventHandlerLCZDecontaminate, IEventHandlerTeamRespawn, IEventHandlerCassieTeamAnnouncement
    {
        public Dictionary<string, int> QueuePoints = new Dictionary<string, int>();
        Random random = new Random();
        List<Task> tasks = new List<Task>();
        private int[] actualHunters;
        #region Settings
        public override void EventStart(RoundStartEvent ev)
        {
            List<Door> doors = ev.Server.Map.GetDoors();
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_A").Locked = true;
            doors.Find(x => x.Name == "CHECKPOINT_LCZ_B").Locked = true;
            foreach (Door door in doors)
            {
                Vector vec = door.Position;
                int rng = random.Next(0, 6);
                for (int i = 0; i < rng; i++)
                {
                    int x = random.Next(Convert.ToInt32(vec.x - 15f), Convert.ToInt32(vec.x + 15f));
                    int z = random.Next(Convert.ToInt32(vec.z - 15f), Convert.ToInt32(vec.z + 15f));
                    int y = Convert.ToInt32(vec.y + 1);
                    Vector spanw_pos = new Vector(x, y, z);

                    Array values = Enum.GetValues(typeof(Props));
                    ItemType item = (ItemType)values.GetValue(random.Next(values.Length));
                    ev.Server.Map.SpawnItem(item, spanw_pos, Vector.Zero);
                }
            }
            actualHunters = GetHunterId();
            foreach (Player player in ev.Server.GetPlayers())
            {
                if (actualHunters.Contains(player.PlayerId))
                    continue;
                player.ChangeRole(RoleType.CLASSD);
                player.SetHealth(50);
                player.PersonalBroadcast(30, Translation["props_spawn"], false);
                Array values = Enum.GetValues(typeof(Props));
                ItemType itemtype = (ItemType)values.GetValue(random.Next(values.Length));
                Item item = ev.Server.Map.GetItems(itemtype, true)[0];
                tasks.Add(Follow(player, item));
            }
            tasks.ForEach(x => x.GetAwaiter());
            foreach (int index in actualHunters)
            {
                Player player = PluginHandler.Shared.Server.GetPlayer(index);
                player.ChangeRole(RoleType.TUTORIAL);
                player.PersonalBroadcast(30, Translation["hunters_wait"], false);
            }
            HuntersWait().GetAwaiter();
        }

        public override string[] GetCommands()
        {
            return new string[] { "prop", "prophunt", "ph" };
        }

        public override string GetName()
        {
            return "Prop Hunt";
        }

        public override void Dispose()
        {
            actualHunters = null;
            tasks.ForEach(x => x.Dispose());
            tasks = new List<Task>();
        }
        #endregion
        private async Task HuntersWait()
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            Door door = PluginHandler.Shared.Server.Map.GetDoors().Find(x => x.Name == "914");
            door.Open = true;
            foreach (int index in actualHunters)
            {
                Player player = PluginHandler.Shared.Server.GetPlayer(index);
                player.ChangeRole(RoleType.SCP_939_53);
                await Task.Delay(500);
                player.Teleport(door.Position);
            }

        }
        private int[] GetHunterId()
        {
            List<int> nums = new List<int>();
            for (int i = 0; i <= PluginHandler.Shared.Server.GetPlayers().Count / 10; i++)
            {
                Player most_player = null;
                int most_points = 0;
                PluginHandler.Shared.Server.GetPlayers().ForEach(player =>
                {
                    if (!QueuePoints.ContainsKey(player.UserId))
                        QueuePoints.Add(player.UserId, 0);
                    if (QueuePoints[player.UserId] >= most_points)
                    {
                        most_player = player;
                        most_points = QueuePoints[player.UserId];
                    }
                    QueuePoints[player.UserId]++;
                });
                QueuePoints[most_player.UserId] = 0;
                nums.Add(most_player.PlayerId);
            }
            return nums.ToArray();
        }

        private async Task Follow(Player player, Item item)
        {
            player.GiveItem(ItemType.MEDKIT).Drop();
            item.SetKinematic(false);
            while(true)
            {
                if (Vector.Distance(item.GetPosition(), player.GetPosition()) > 1f)
                {
                    if (item.GetKinematic())
                        item.SetKinematic(false);
                    Vector vector = new Vector(player.GetPosition().x, player.GetPosition().y - 0.5f, player.GetPosition().z);
                    item.SetPosition(vector);
                }
                else if(!item.GetKinematic())
                    item.SetKinematic(true);
                await Task.Delay(1000 / 8);
            }
        }

        public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
        {
            if (!isQueue)
                return;
            ev.Item.Drop();
        }

        public void OnDecontaminate()
        {
            if (!isQueue)
                return;
            foreach (int index in actualHunters)
            {
                Player player = PluginHandler.Shared.Server.GetPlayer(index);
                player.Kill();
            }
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            if (!isQueue)
                return;
            ev.PlayerList.ForEach(x => x.ChangeRole(RoleType.SPECTATOR));
        }

        public void OnCassieTeamAnnouncement(CassieTeamAnnouncementEvent ev)
        {
            if (!isQueue)
                return;
            ev.Allow = false;
        }

        public enum Props
        {
            MEDKIT = 14,
            RADIO = 12,
            DISARMER = 27,
            SCP268 = 32,
            PAINKILLERS = 34,
            WEAPON_MANAGER_TABLET = 19,
            AMMO9MM = 29,
            USP = 30
        }
    }
}
