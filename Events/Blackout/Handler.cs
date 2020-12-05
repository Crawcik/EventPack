using EventManager;
using Smod2;
using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;
using Smod2.EventSystem.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blackout
{
    class Handler : GameEvent, IEventHandlerTeamRespawn
    {
        private bool Blinking;
        private Dictionary<int, int> player_data = new Dictionary<int, int>();

        #region Settings
        public override void Register()
        {
            DefaultTranslation = new Dictionary<string, string> {
                { "battery", "Battery" }
            };
        }

        public override string[] GetCommands() => new[] { "blackout", "black", "darkness" };

        public override string GetName() => "Blackout";
        #endregion

        public override void EventStart(RoundStartEvent ev)
        {
            Blinking = true;
            BlinkingLight().GetAwaiter();
            GiveFlashlight(ev.Server.GetPlayers()).GetAwaiter();
            CheckPlayers().GetAwaiter();
        }

        public override void EventEnd(RoundEndEvent ev)
        {
            Blinking = false;
        }

        private async Task BlinkingLight()
        {
            while (Blinking)
            {
                PluginManager.Manager.Server.Map.OverchargeLights(30f, false);
                await Task.Delay(System.TimeSpan.FromSeconds(31));
            }
        }

        public async Task CheckPlayers()
        {
            while (Blinking)
            {
                foreach (Player x in PluginManager.Manager.Server.GetPlayers().FindAll(x => x.TeamRole.Team != TeamType.SPECTATOR && x.TeamRole.Team != TeamType.SCP))
                {
                    //Sprawdzanie i wykonywanie anomali zdrowego
                    if (!player_data.ContainsKey(x.PlayerId))
                        player_data.Add(x.PlayerId, 100);
                    if (x.GetCurrentItem().ItemType == ItemType.FLASHLIGHT)
                        player_data[x.PlayerId] -= 2;
                    if (player_data[x.PlayerId] < 0)
                    {
                        player_data[x.PlayerId] = 0;
                        x.SetCurrentItemIndex(7);
                    }
                    else if (player_data[x.PlayerId] < 100)
                        player_data[x.PlayerId]++;
                    x.PersonalBroadcast(1, $"{Translation("battery")} {player_data[x.PlayerId]}%", true);
                }
                await Task.Delay(1000);
            }
        }

        public async Task GiveFlashlight(List<Player> players)
        {
            await Task.Delay(500);
            foreach (Player x in players)
            {
                x.GiveItem(ItemType.FLASHLIGHT);
                if (!player_data.ContainsKey(x.PlayerId))
                    player_data.Add(x.PlayerId, 100);
                else
                    player_data[x.PlayerId] = 100;
            }
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            GiveFlashlight(ev.PlayerList).GetAwaiter();
        }
    }
}
