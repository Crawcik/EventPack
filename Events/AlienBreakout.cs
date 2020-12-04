using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventManager.Events
{
    public class AlienBreakout : Event, IEventHandlerRoundRestart, IEventHandlerTeamRespawn
    {
        #region Settings
        private bool Blinking = false;
        private Dictionary<int, int> innocent_data = new Dictionary<int, int>();
        public override string[] GetCommands()
        {
            return new string[] { "alien", "breakout" };
        }

        public override string GetName()
        {
            return "Alien Breakout";
        }
        #endregion
        public override void EventStart(RoundStartEvent ev)
        {
            Blinking = true;
            BlinkingLight().GetAwaiter();
            ev.Server.GetPlayers().ForEach(x => x.GiveItem(ItemType.FLASHLIGHT));
            CheckPlayers().GetAwaiter();
        }

        public void OnRoundRestart(RoundRestartEvent ev)
        {
            Blinking = false;
        }

        private async Task BlinkingLight()
        {
            while (Blinking)
            {
                PluginHandler.Shared.Server.Map.OverchargeLights(30f, false);
                await Task.Delay(TimeSpan.FromSeconds(31));
            }
        }

        public async Task CheckPlayers()
        {
            while (Blinking)
            {
                foreach (Player x in PluginHandler.Shared.Server.GetPlayers().FindAll(x => x.TeamRole.Team != TeamType.SPECTATOR && x.TeamRole.Team != TeamType.SCP))
                {
                    //Sprawdzanie i wykonywanie anomali zdrowego
                    if (!innocent_data.ContainsKey(x.PlayerId))
                        innocent_data.Add(x.PlayerId, 100);
                    if (x.GetCurrentItem().ItemType == ItemType.FLASHLIGHT)
                        innocent_data[x.PlayerId] -= 2;
                    if (innocent_data[x.PlayerId] < 0) {
                        innocent_data[x.PlayerId] = 0;
                        x.SetCurrentItemIndex(7);
                    }
                    else if (innocent_data[x.PlayerId] < 100)
                        innocent_data[x.PlayerId]++;
                    x.PersonalBroadcast(1, $"Bateria {innocent_data[x.PlayerId]}%", true);
                }
                await Task.Delay(1000);
            }
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            if (!isQueue)
                return;
            foreach (Player x in ev.PlayerList)
            {
                x.GiveItem(ItemType.FLASHLIGHT);
                if (!innocent_data.ContainsKey(x.PlayerId))
                    innocent_data.Add(x.PlayerId, 100);
                else
                    innocent_data[x.PlayerId] = 100;
            }
        }

        public override void Dispose()
        {
            innocent_data.Clear();
        }
    }
}
