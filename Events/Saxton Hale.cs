using Smod2.API;
using Smod2.Commands;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Events
{
    public class Saxton_Hale : Event, IEventHandlerRoundStart, IEventHandlerCheckRoundEnd, IEventHandlerSpawn, IEventHandlerWaitingForPlayers
    {
        public Dictionary<string, int> QueuePoints = new Dictionary<string, int>();
        private Boss boss = null;
        public int boss_type_num = 0;
        #region Setting
        public Saxton_Hale()
        {
            this.Translation = PluginHandler.Shared.AllTranslations[GetName()];
        }

        public override string[] GetCommands()
        {
            return new[] { "event_hale", "event_saxton", "event_saxtonhale" };
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

        public void OnRoundStart(RoundStartEvent ev)
        {
            if (!isQueue)
                return;
            //Setting map
            ev.Server.Map.GetElevators().ForEach(x => x.Locked = true);
            //Selecting player
            Player most_player = null;
            int most_points = 0;
            ev.Server.GetPlayers().ForEach(player =>
            {
                if (!QueuePoints.ContainsKey(player.UserId))
                    QueuePoints.Add(player.UserId, 0);
                if (QueuePoints[player.UserId] >= most_points)
                {
                    most_player = player;
                    most_points = QueuePoints[player.UserId];
                }
                QueuePoints[player.UserId]++;
                player.ChangeRole(Smod2.API.Role.NTF_LIEUTENANT);
                player.GetInventory().ForEach(x => x.Remove());
                player.GiveItem(Smod2.API.ItemType.GUNE11SR);
                player.GiveItem(Smod2.API.ItemType.GRENADEFRAG);
                player.GiveItem(Smod2.API.ItemType.ADRENALINE);
                player.SetAmmo(AmmoType.DROPPED_5, 1000);
            });
            QueuePoints[most_player.UserId] = 0;
            //Setting boss
            if (boss_type_num == 2)
                boss_type_num = 0;
                this.boss = new Boss((Boss.Class)boss_type_num, most_player);
            boss_type_num++;
            this.boss.player.SetHealth(ev.Server.GetPlayers().Count * 600);
            most_player = null;
        }

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            if (!isQueue || boss == null)
                return;
            if (ev.Status == ROUND_END_STATUS.ON_GOING)
            {
                ev.Server.Map.ClearBroadcasts();
                ev.Server.Map.Broadcast(2, boss.player.Name + " become " + boss.role.ToString() + " | Boss HP: " + boss.player.GetHealth(), false);
            }
            else
            {
                if (boss.player.TeamRole.Role != Smod2.API.Role.CHAOS_INSURGENCY)
                {
                    boss.EndTask();
                    boss = null;
                }
            }
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            if (!isQueue)
                return;
            if (ev.Player.TeamRole.Role == Smod2.API.Role.CLASSD)
                ev.Player.ChangeRole(Smod2.API.Role.SPECTATOR);
        }

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            if (!isQueue)
                return;
            ev.Server.Map.Broadcast(20, Translation["tutorial"], false);
        }

        private class Boss
        {
            public Player player;
            public Class role;
            private List<Abbility> ActiveAbbilities;
            private bool onGoing = true;
            public Boss(Class _class, Player player)
            {
                this.role = _class;
                this.player = player;
                this.ActiveAbbilities = new List<Abbility>() { Abbility.RAGE, Abbility.TAUNT, Abbility.SPECIAL };
                this.player.ChangeRole(Smod2.API.Role.CHAOS_INSURGENCY);
                Handle().GetAwaiter();
                SetNormalInventory();
            }

            public void EndTask()
            {
                onGoing = false;
                ActiveAbbilities.Clear();
            }

            private async Task Handle()
            {
                while (onGoing)
                {
                    switch ((int)player.GetCurrentItem().ItemType)
                    {
                        case (int)Abbility.RAGE:
                            int hp = player.GetHealth();
                            Vector vector = this.player.GetPosition();
                            ActiveAbbilities.Remove(Abbility.RAGE);
                            await Task.Delay(50);
                            this.player.ChangeRole(Smod2.API.Role.SCP_096);
                            await Task.Delay(50);
                            this.player.Teleport(vector);
                            this.player.SetHealth(hp);
                            await Task.Delay(17000);
                            vector = this.player.GetPosition();
                            hp = player.GetHealth();
                            await Task.Delay(50);
                            this.player.ChangeRole(Smod2.API.Role.CHAOS_INSURGENCY);
                            await Task.Delay(50);
                            this.player.Teleport(vector);
                            this.player.SetHealth(hp);
                            SetNormalInventory();
                            break;
                        case (int)Abbility.TAUNT:
                            PluginHandler.Shared.Server.Map.Shake();
                            ActiveAbbilities.Remove(Abbility.TAUNT);
                            PluginHandler.Shared.Server.GetPlayers(Smod2.API.Role.NTF_LIEUTENANT).ForEach(x => x.GetInventory().ForEach(y => y.Drop()));
                            SetNormalInventory();
                            break;
                        case (int)Abbility.SPECIAL:
                            ActiveAbbilities.Remove(Abbility.SPECIAL);
                            SpecialAbbility().Start();
                            SpecialAbbility().Wait();
                            break;
                        case (int)Smod2.API.ItemType.GUNE11SR:
                            player.GetInventory().Find(x => x.ItemType == Smod2.API.ItemType.GUNE11SR).Drop();
                            break;
                        
                    }
                    await Task.Delay(500);
                }
            }

            private async Task SpecialAbbility()
            {
                switch (role)
                {
                    case Class.SAXTON:
                        this.player.GetInventory().ForEach(x => x.Remove());
                        player.SetGodmode(true);
                        player.GiveItem(Smod2.API.ItemType.GUNLOGICER);
                        await Task.Delay(100);
                        player.SetCurrentItem(Smod2.API.ItemType.GUNLOGICER);
                        await Task.Delay(10000);
                        player.SetGodmode(false);
                        SetNormalInventory();
                        break;
                    case Class.RIPPER:
                        Vector vector = this.player.GetPosition();
                        PluginHandler.Shared.Server.GetPlayers(Smod2.API.Role.SPECTATOR).ForEach(x => {
                            x.ChangeRole(Smod2.API.Role.ZOMBIE);
                            x.Teleport(vector);
                        });
                        break;
                    case Class.GHOST:
                        SetNormalInventory();
                        this.player.SetGhostMode(true);
                        await Task.Delay(10000);
                        this.player.SetGhostMode(false);
                        break;
                }
            }

            private void SetNormalInventory() {
                this.player.GetInventory().ForEach(x => x.Remove());
                ActiveAbbilities.ForEach(x => player.GiveItem((Smod2.API.ItemType)x));
                player.GiveItem(Smod2.API.ItemType.GUNUSP);
                this.player.SetAmmo(AmmoType.DROPPED_9, 1000);
                this.player.SetCurrentItem(Smod2.API.ItemType.GUNUSP);
            }

            public enum Class
            {
                SAXTON,
                RIPPER,
                GHOST
            }

            public enum Abbility
            {
                RAGE = Smod2.API.ItemType.KEYCARDJANITOR,
                TAUNT = Smod2.API.ItemType.KEYCARDNTFCOMMANDER,
                SPECIAL = Smod2.API.ItemType.KEYCARDO5
            }
        }
    }
}
