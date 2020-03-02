using Smod2.API;
using Smod2.Commands;
using Smod2.EventHandlers;
using Smod2.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManager.Events
{
    class SaxtonHale : Event, IEventHandlerWaitingForPlayers, IEventHandlerRoundStart
    {
        private PluginHandler plugin;
        private bool isWait;
        private Boss boss;
        private List<QueuePoints> queues= new List<QueuePoints>();
        private int num_bos = 0;
        List<Player> players;
        Random random = new Random();

        #region Settings
        public SaxtonHale(PluginHandler plugin)
        {
            this.plugin = plugin;
        }

        public override string[] GetCommands()
        {
            return new string[] { "event_hale" };
        }

        public override ConsoleType GetCommandType()
        {
            return ConsoleType.RA;
        }

        public override string GetName()
        {
            return "Saxton Hale";
        }
        #endregion

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            if (!isQueue)
                return;
            isWait = true;
            Print("Działanie Boss'a: Karta Fioletowa - RAGE, Karta niebieska - TAUNT, Karta O5 - Special").GetAwaiter();
        }

        private async Task Print(string msg)
        {
            while (isWait)
            {
                plugin.CommandManager.CallCommand(plugin.Server, "bc", new string[] { "1", msg });
                await Task.Delay(1000);
            }
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            if (!isQueue)
                return;
            isWait = false;
            string pretendend = "";
            int pretendPoints = -1;
            players = ev.Server.GetPlayers();
            ev.Server.GetPlayers().Clear();
            //Finding pretendend
            foreach (Player player in players)
            {
                if (queues.Count == 0)
                {
                    queues.Add(new QueuePoints(player.UserId, 0));
                } else if (!queues.Exists(x => x.steamID == player.UserId))
                {
                    queues.Add(new QueuePoints(player.UserId, 0));
                }
                QueuePoints s = queues.Find(x => x.steamID == player.UserId);
                queues.Find(x => x.steamID == player.UserId).queue += 10;
                if (s.queue > pretendPoints)
                {
                    pretendend = s.steamID;
                    pretendPoints = s.queue;
                }
            }
            //Setting boss
            Array array = Enum.GetValues(typeof(Boss.Characters));
            num_bos = (int)array.GetValue(random.Next(0, array.Length - 1));
            boss = new Boss(num_bos, players.Find(x => x.UserId == pretendend));
            boss.Player.ChangeRole(Smod2.API.Role.CHAOS_INSURGENCY);
            queues.Find(x => x.steamID == pretendend).queue = 0;
            plugin.Server.Map.Broadcast(5, $"{boss.Player.Name} become {Enum.GetName(typeof(Boss.Characters), boss.CurrentClass)}", false);
            boss.Player.SetHealth((players.Count - 1) * 600);
            //Setting players
            foreach (Player player1 in players.Where(x => x.PlayerId != boss.Player.PlayerId))
            {
                player1.ChangeRole(Smod2.API.Role.NTF_LIEUTENANT, spawnProtect: false);
                player1.GetInventory().ForEach(x => x.Remove());
                player1.SetAmmo(AmmoType.DROPPED_5, 1000);
                player1.GiveItem(Smod2.API.ItemType.GUNE11SR);
                player1.GiveItem(Smod2.API.ItemType.GRENADEFRAG);
            }
            isWait = false;
            //Setting map
            
            plugin.Server.Map.GetDoors().Find(x => x.Name == "NUKE_SURFACE").Locked = true;
            foreach (Elevator elevator in plugin.Server.Map.GetElevators())
            {
                elevator.Locked = true;
            }
            plugin.Server.Map.Shake();
            Check_boss().GetAwaiter();
        }
        private bool CheckBossDead(int PlayerID)
        {
            if(boss.Player.TeamRole.Role == Smod2.API.Role.SPECTATOR || boss.Player.TeamRole.Role == Smod2.API.Role.UNASSIGNED)
                return false;
            return true;
        }
        private async Task Check_boss()
        {
            try
            {
                boss.Player.GetInventory().ForEach(x => x.Remove());
                boss.Player.GiveItem(Smod2.API.ItemType.GUNUSP);
                boss.Player.GiveItem(Smod2.API.ItemType.KEYCARDJANITOR);
                boss.Player.GiveItem(Smod2.API.ItemType.KEYCARDNTFCOMMANDER);
                boss.Player.GiveItem(Smod2.API.ItemType.KEYCARDO5);
                boss.Player.SetCurrentItem(Smod2.API.ItemType.GUNUSP);
                boss.Player.SetAmmo(AmmoType.DROPPED_9, 3000);
                await Task.Delay(5000);
                while (CheckBossDead(boss.Player.PlayerId))
                {
                    await Task.Delay(1000);
                    boss.ExtendedHP = boss.Player.GetHealth();
                    plugin.Server.Map.Broadcast(1, $"HP: {boss.ExtendedHP}", false);
                    if (boss.Player.HasItem(Smod2.API.ItemType.GUNE11SR))
                        boss.Player.GetInventory().Find(x => x.ItemType == Smod2.API.ItemType.GUNE11SR).Drop();
                    if (boss.Player.GetCurrentItem().ItemType == Smod2.API.ItemType.KEYCARDJANITOR)
                    {
                        boss.Player.GetCurrentItem().Remove();
                        boss.Player.SetCurrentItem(Smod2.API.ItemType.NONE);
                        boss.Rage().GetAwaiter();
                    }
                    if (boss.Player.GetCurrentItem().ItemType == Smod2.API.ItemType.KEYCARDNTFCOMMANDER)
                    {
                        boss.Player.GetCurrentItem().Remove();
                        boss.Taunt(players.Where(x => x.TeamRole.Role != Smod2.API.Role.SPECTATOR && x.PlayerId != boss.Player.PlayerId).ToArray());
                        plugin.Server.Map.Shake();
                        plugin.Server.Map.ClearBroadcasts();
                        plugin.Server.Map.Broadcast(1, $"TAUNT!!!", false);
                    }
                    if (boss.Player.GetCurrentItem().ItemType == Smod2.API.ItemType.KEYCARDO5)
                    {
                        boss.Player.GetCurrentItem().Remove();
                        boss.Player.SetCurrentItem(Smod2.API.ItemType.NONE);
                        boss.Special((int)boss.CurrentClass, plugin.Server.GetPlayers());
                    }
                }
            }
            catch (Exception ev)
            {
                plugin.Error($"SRC:{ev.Source} ||| MSG: {ev.Message}");
            }
        }

        private class QueuePoints
        {
            public QueuePoints(string v1, int v2)
            {
                this.steamID = v1;
                this.queue = v2;
            }

            public string steamID { get; private set; }
            public int queue { get; set; }
        }

        public class Boss
        {
            public Player Player;

            public Boss(int boss_type,Player player)
            {
                this.CurrentClass = boss_type;
                this.Player = player;
                powerAvalible[0] = true;
                powerAvalible[1] = true;
                powerAvalible[2] = true;
            }

            public async Task AdditionalAbbility(int num, List<Player> additional)
            {
                powerAvalible[2] = false;
                if (num == 0)
                {
                    this.Player.SetAmmo(AmmoType.DROPPED_7, 200);
                    this.Player.SetGodmode(true);
                    this.Player.GiveItem(Smod2.API.ItemType.GUNLOGICER);
                    await Task.Delay(400);
                    this.Player.SetCurrentItem(Smod2.API.ItemType.GUNLOGICER);
                    await Task.Delay(10000);
                    this.Player.GetInventory().Find(x => x.ItemType == Smod2.API.ItemType.GUNLOGICER).Remove();
                    this.Player.SetGodmode(false);
                    await Task.Delay(400);
                    this.Player.SetCurrentItem(Smod2.API.ItemType.GUNUSP);
                }
                if (num == 1)
                {
                    Random random = new Random();
                    this.Player.SetGodmode(true);
                    this.Player.SetCurrentItem(Smod2.API.ItemType.GUNUSP);
                    for (int i = 0; i < 80; i++)
                    {
                        random.Next(0, 360);
                        Vector v = new Vector(60, random.Next(0, 360), 0);
                        Player.ThrowGrenade(GrenadeType.FRAG_GRENADE, true, v, true, Player.GetPosition(), true, 3f);
                        await Task.Delay(150);
                    }
                    this.Player.SetGodmode(false);
                }
                if (num == 2)
                {
                    Player[] players = additional.FindAll(x => x.TeamRole.Role == Smod2.API.Role.SPECTATOR).ToArray();
                    additional.Clear();
                    foreach (Player player in players)
                    {
                        player.ChangeRole(Smod2.API.Role.SCP_049_2);
                        player.Teleport(this.Player.GetPosition(), true);
                    }
                }
            }

            public void DeAbbility()
            {
                
                if (this.Player.TeamRole.Role != Smod2.API.Role.CHAOS_INSURGENCY) {
                    Vector vector = this.Player.GetPosition();
                    this.Player.ChangeRole(Smod2.API.Role.CHAOS_INSURGENCY, spawnTeleport: false, spawnProtect: false);
                    this.Player.Teleport(vector);
                }
                this.Player.GetInventory().ForEach(x => x.Remove());
                this.Player.GiveItem(Smod2.API.ItemType.GUNUSP);
                if (powerAvalible[0])
                {
                    this.Player.GiveItem(Smod2.API.ItemType.KEYCARDJANITOR);
                }
                if (powerAvalible[1])
                {
                    this.Player.GiveItem(Smod2.API.ItemType.KEYCARDNTFCOMMANDER);
                }
                if (powerAvalible[2])
                {
                    this.Player.GiveItem(Smod2.API.ItemType.KEYCARDO5);
                }
                this.Player.SetCurrentItem(Smod2.API.ItemType.GUNUSP);
                this.Player.SetAmmo(AmmoType.DROPPED_9, 3000);
                this.Player.SetCurrentItem(Smod2.API.ItemType.GUNUSP);
                this.Player.SetHealth(ExtendedHP);
            }
            public void Special(int use, List<Player> additional)
            {
                AdditionalAbbility(use, additional).GetAwaiter();
            }

            public async Task Rage()
            {
                Vector vector = this.Player.GetPosition();
                powerAvalible[0] = false;
                this.Player.ChangeRole(Smod2.API.Role.SCP_096, spawnTeleport: false, spawnProtect: false);
                this.Player.SetHealth(ExtendedHP);
                this.Player.Teleport(vector);
                await Task.Delay(16000);
                DeAbbility();
            }

            public void Taunt(Player[] players_to_handcuff)
            {
                powerAvalible[1] = false;
                foreach (Player pl in players_to_handcuff)
                    pl.GetInventory().ForEach(x => x.Drop());

            }
            public int CurrentClass { get; private set; }

            public bool[] powerAvalible = new bool[3];
            public int ExtendedHP { get; set; }

            public enum Characters
            {
                SAXTON = 0,
                GRABARZ = 2
            }
        }
    }
}
