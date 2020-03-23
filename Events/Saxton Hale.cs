using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManager.Events
{
    public class Saxton_Hale : Event, IEventHandlerRoundStart, IEventHandlerCheckRoundEnd
    {
        public Dictionary<string, int> QueuePoints = new Dictionary<string, int>();
        private Boss boss = null;
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

        public override void Dispose()
        {
            boss = null;
        }
        #endregion

        public void OnRoundStart(RoundStartEvent ev)
        {
            //Selecting player
            Player  most_player = null;
            int most_points = 0;
            ev.Server.GetPlayers().ForEach(player =>
            {
                if (QueuePoints.ContainsKey(player.UserId))
                    QueuePoints.Add(player.UserId, 0);
                if (QueuePoints[player.UserId] >= most_points)
                {
                    most_player = player;
                    QueuePoints[player.UserId] = most_points;
                }
                QueuePoints[player.UserId]++;
                player.ChangeRole(Smod2.API.Role.NTF_LIEUTENANT);
                player.GetInventory().ForEach(x => x.Remove());
                player.GiveItem(Smod2.API.ItemType.GUNE11SR);
                player.GiveItem(Smod2.API.ItemType.GRENADEFRAG);
                player.SetAmmo(AmmoType.DROPPED_5, 1000);
            });
            QueuePoints[most_player.UserId] = 0;

            //Setting boss
            Boss boss = new Boss(Boss.Class.SAXTON, most_player);
            most_player = null;
        }

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            if (ev.Status == ROUND_END_STATUS.ON_GOING)
            {
                ev.Server.Map.ClearBroadcasts();
                ev.Server.Map.Broadcast(2, "Boss HP: " + boss.player.GetHealth(), false);
            }
            else
            {
                if (boss != null)
                    boss.EndTask();
            }
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
                SetNormalInventory();
                Handle().GetAwaiter();
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
                            player.SetCurrentItem(Smod2.API.ItemType.NONE);
                            ActiveAbbilities.Remove(Abbility.RAGE);
                            player.ChangeRole(Smod2.API.Role.SCP_096);
                            await Task.Delay(10000);
                            player.ChangeRole(Smod2.API.Role.CHAOS_INSURGENCY);
                            SetNormalInventory();
                            break;
                        case (int)Abbility.TAUNT:
                            ActiveAbbilities.Remove(Abbility.TAUNT);
                            PluginHandler.Shared.Server.GetPlayers(Smod2.API.Role.NTF_LIEUTENANT).ForEach(x => x.GetInventory().ForEach(y => y.Drop()));
                            SetNormalInventory();
                            break;
                        case (int)Abbility.SPECIAL:
                            ActiveAbbilities.Remove(Abbility.SPECIAL);
                            SpecialAbbility().Start();
                            SpecialAbbility().Wait();
                            break;
                        case (int)Smod2.API.ItemType.GUNUSP:
                            break;
                        default:
                            SetNormalInventory();
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
                        player.SetGodmode(true);
                        player.GiveItem(Smod2.API.ItemType.GUNLOGICER);
                        player.SetCurrentItem(Smod2.API.ItemType.GUNLOGICER);
                        await Task.Delay(10000);
                        player.SetGodmode(false);
                        SetNormalInventory();
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
                DEMOMAN,
                RIPPER
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
